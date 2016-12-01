using System.Collections.ObjectModel;
using Bindable.Linq;
using Bindable.Linq.Interfaces;

namespace EditableCollection
{
    public class CustomerRepository
    {
        private readonly ObservableCollection<Customer> _customers;

        public CustomerRepository()
        {
            _customers = new ObservableCollection<Customer>();
            _customers.Add(new Customer { FirstName = "Angela", LastName = "Adams", Age = 29 });
            _customers.Add(new Customer { FirstName = "Benny", LastName = "Bernanke", Age = 34 });
            _customers.Add(new Customer { FirstName = "Charlie", LastName = "Chaplin", Age = 19 });
            _customers.Add(new Customer { FirstName = "Darryl", LastName = "Donkey Kong", Age = 11 });
            _customers.Add(new Customer { FirstName = "Edgar", LastName = "Eucalyptus", Age = 20 });
        }

        public IBindableCollection<Customer> GetCustomers()
        {
            return _customers.AsBindable();
        }

        public void SaveCustomer(Customer customer)
        {
            if (!_customers.Contains(customer))
            {
                _customers.Add(customer);
            }
        }

        public void DeleteCustomer(Customer customer)
        {
            if (_customers.Contains(customer))
            {
                _customers.Remove(customer);
            }
        }
    }
}
