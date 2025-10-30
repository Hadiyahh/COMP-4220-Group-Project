using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Controls;
using BookStoreLIB;

namespace BookStoreGUI.Views
{
    public partial class InventoryView : UserControl
    {
        private readonly BookRepository _repo = new BookRepository();
        public ObservableCollection<Book> Items { get; } = new ObservableCollection<Book>();

        public InventoryView()
        {
            InitializeComponent();
            BooksGrid.DataContext = Items;
            Loaded += async (_, __) => await LoadAsync();
        }

        private async Task LoadAsync()
        {
            Items.Clear();
            var rows = await _repo.GetAllAsync();
            foreach (var b in rows) Items.Add(b);
        }
    }
}
