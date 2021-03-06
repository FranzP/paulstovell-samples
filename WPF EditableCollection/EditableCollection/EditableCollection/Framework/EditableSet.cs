﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Bindable.Linq;
using Bindable.Linq.Interfaces;
using BindingOriented.Adapters;
using System.Windows.Input;

namespace EditableCollection.Framework
{
    // Composite WPF provides this command - I've duplicated a simple version here
    public class DelegateCommand<TElement> : ICommand
    {
        private readonly Action<TElement> _execute;
        private readonly Func<TElement, bool> _canExecute;

        public DelegateCommand(Action<TElement> executedAction, Func<TElement, bool> canExecute)
        {
            _execute = executedAction;
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute((TElement) parameter);
        }

        public event EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            if (_execute != null)
            {
                _execute((TElement) parameter);
            }
        }
    }

    public class DelegateCommand : DelegateCommand<object>
    {
        public DelegateCommand(Action<object> executedAction, Func<object, bool> canExecute) : base(executedAction, canExecute)
        {
        }
    }

    public class EditableSet<TElement> where TElement : class, new()
    {
        private readonly IBindableCollection<TElement> _originals;
        private readonly IBindableCollection<Editable<TElement>> _editableOriginals;
        private readonly ObservableCollection<Editable<TElement>> _inserted;
        private readonly DelegateCommand<Editable<TElement>> _undoItemChangesCommand;
        private readonly DelegateCommand<Editable<TElement>> _deleteItemCommand;
        private readonly DelegateCommand _undoChangesCommand;
        private readonly DelegateCommand _commitChangesCommand;
        private readonly Action<TElement> _editItemAction;
        private readonly Action<TElement> _addItemAction;
        private readonly Action<TElement> _deleteItemAction;

        public EditableSet(IBindableCollection<TElement> originals, Action<TElement> editItemAction, Action<TElement> addItemAction, Action<TElement> deleteItemAction)
        {
            _originals = originals;
            _deleteItemAction = deleteItemAction;
            _addItemAction = addItemAction;
            _editItemAction = editItemAction;

            _inserted = new ObservableCollection<Editable<TElement>>();
            _editableOriginals = _originals.AsBindable().Select(orig => new Editable<TElement>(orig));
            _editableOriginals.Evaluate();

            _undoItemChangesCommand = new DelegateCommand<Editable<TElement>>(
                item => UndoItemChanges(item),
                item => item.HasChanges
                );
            _deleteItemCommand = new DelegateCommand<Editable<TElement>>(
                item => DeleteItem(item),
                item => true
                );
            _undoChangesCommand = new DelegateCommand(
                ignored => UndoChanges(),
                ignored => true
                );
            _commitChangesCommand = new DelegateCommand(
                ignored => CommitChanges(),
                ignored => true
                );
        }

        public ICommand DeleteItemCommand
        {
            get { return _deleteItemCommand; }
        }

        public ICommand UndoItemChangesCommand
        {
            get { return _undoItemChangesCommand; }
        }

        public ICommand UndoChangesCommand
        {
            get { return _undoChangesCommand; }
        }

        public ICommand CommitChangesCommand
        {
            get { return _commitChangesCommand; }
        }

        public IBindableCollection<TElement> OriginalItems
        {
            get { return _originals; }
        }

        public IBindableCollection<Editable<TElement>> EditableItems
        {
            get
            {
                return AllItems.Where(e => e.IsDeleted == false);
            }
        }

        public IBindableCollection<Editable<TElement>> AllItems
        {
            get
            {
                var items = _editableOriginals;
                items = items.Union(_inserted.AsBindable());
                items.Evaluate();
                return items;
            }
        }

        public IBindableCollection<Editable<TElement>> ChangedItems
        {
            get { return EditableItems.Where(editable => editable.HasChanges); }
        }

        public IBindableCollection<Editable<TElement>> DeletedItems
        {
            get { return _editableOriginals.Where(editable => editable.IsDeleted); }
        }

        public ObservableCollection<Editable<TElement>> Inserted
        {
            get { return _inserted; }
        }

        public void UndoChanges()
        {
            foreach (var editable in ChangedItems.ToArray())
            {
                editable.UndoChanges();
            }
            foreach (var deleted in DeletedItems.ToArray())
            {
                deleted.UndoChanges();
            }
            _inserted.Clear();
        }

        public void UndoItemChanges(Editable<TElement> item)
        {
            item.UndoChanges();
            if (_inserted.Contains(item))
            {
                _inserted.Remove(item);
            }
        }

        public void AddNewItem(Editable<TElement> item)
        {
            item.IsNew = true;
            _inserted.Add(item);
        }

        private void DeleteItem(Editable<TElement> item)
        {
            item.IsDeleted = true;
        }

        public TElement[] CommitChanges()
        {
            var results = new List<TElement>();
            foreach (var editable in ChangedItems.ToArray())
            {
                editable.CommitChanges();
                results.Add(editable.Original);
            }
            foreach (var inserted in Inserted.ToArray())
            {
                inserted.CommitChanges();
                results.Add(inserted.Original);
            }
            _inserted.Clear();

            return results.ToArray();
        }
    }
}
