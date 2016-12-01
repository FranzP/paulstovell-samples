using System.Linq;
using System.Windows;
using System.Windows.Controls;
using BindingOriented.Adapters;
using EditableCollection.Framework;
using Xceed.Wpf.DataGrid;

namespace EditableCollection
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        private readonly CustomerRepository _customerRepository;
        private readonly EditableSet<Customer> _editableSet;

        public Window1()
        {
            InitializeComponent();

            _customerRepository = new CustomerRepository();

            _editableSet = new EditableSet<Customer>(
                _customerRepository.GetCustomers(),
                edit => _customerRepository.SaveCustomer(edit),
                add => _customerRepository.SaveCustomer(add),
                delete => _customerRepository.DeleteCustomer(delete)
                );

            DataContext = _editableSet;
        }

        private bool _showDeletedItems;

        public bool ShowDeletedItems
        {
            get { return _showDeletedItems; }
            set
            {
                _showDeletedItems = value;
                ((DataGridCollectionViewSource)Resources["EditableItems"]).Source = _showDeletedItems 
                    ? _editableSet.AllItems
                    : _editableSet.EditableItems;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Delete items that were marked for deletion
            foreach (var item in _editableSet.DeletedItems.ToArray())
            {
                _customerRepository.DeleteCustomer(item.Original);
            }

            // Commit the changes in memory
            var changed = _editableSet.CommitChanges();

            // Save the edited items
            foreach (var item in changed)
            {
                _customerRepository.SaveCustomer(item);
            }
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            var context = (EditableSet<Customer>)dataGridControl1.DataContext;
            context.UndoChanges();
        }

        private void UndoItemButton_Click(object sender, RoutedEventArgs e)
        {
            var item = ((Button)sender).CommandParameter as Editable<Customer>;
            if (item != null)
            {
                _editableSet.UndoItemChanges(item);
            }
        }

        private void SimulateRemoteUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            _customerRepository.SaveCustomer(new Customer { FirstName = "Mary", LastName = "Mellon" });
        }

        private void CollectionView_CreatingNewItem(object sender, DataGridCreatingNewItemEventArgs e)
        {
            e.NewItem = new Editable<Customer>(new Customer { FirstName = "", LastName = "" }) { IsNew = true };
            e.Handled = true;
        }

        private void CollectionView_CommittingNewItem(object sender, DataGridCommittingNewItemEventArgs e)
        {
            var customer = (Editable<Customer>)e.Item;
            _editableSet.AddNewItem(customer);
            e.NewCount = 1;
            e.Index = 0;
            e.Handled = true;
        }

        private void CollectionView_CancellingNewItem(object sender, DataGridItemHandledEventArgs e)
        {
            e.Handled = true;
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            var cell = menuItem.CommandParameter as DataCell;
            var row = cell.ParentRow as DataRow;
            var customer = row.DataContext as Editable<Customer>;
            if (customer != null)
            {
                customer.IsDeleted = true;
            }
        }
    }
}
