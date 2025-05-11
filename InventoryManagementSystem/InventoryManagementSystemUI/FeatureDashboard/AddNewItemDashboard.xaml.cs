using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace InventoryManagementSystemUI.FeatureDashboard
{
    /// <summary>
    /// Interaction logic for AddNewItemDashboard.xaml
    /// </summary>
    public partial class AddNewItemDashboard : UserControl
    {
        // List to hold categories
        private List<Category> _categories = new List<Category>();
        private bool _isRefreshingTreeView = false;
        public AddNewItemDashboard()
        {
            InitializeComponent();

            // Hook events for placeholders
            HookTextBoxEvents(ItemNameTextBox, ItemNamePlaceholder);
            HookTextBoxEvents(ItemDescriptionTextBox, ItemDescriptionPlaceholder);
            HookTextBoxEvents(QuantityTextBox, QuantityPlaceholder);
            HookTextBoxEvents(PriceTextBox, PricePlaceholder);

            // Load categories into TreeView
            LoadCategories();
        }

        private void HookTextBoxEvents(TextBox textBox, TextBlock placeholder)
        {
            textBox.GotFocus += (s, e) => ToggleTextBoxPlaceholder(textBox, placeholder);
            textBox.LostFocus += (s, e) => ToggleTextBoxPlaceholder(textBox, placeholder);
            textBox.TextChanged += (s, e) => ToggleTextBoxPlaceholder(textBox, placeholder);
        }

        private void ToggleTextBoxPlaceholder(TextBox textBox, TextBlock placeholder)
        {
            placeholder.Visibility = string.IsNullOrEmpty(textBox.Text) && !textBox.IsFocused
                ? Visibility.Visible
                : Visibility.Collapsed;
        }

        private void ToggleTreeViewPlaceholder()
        {
            // Handle visibility of placeholder for TreeView (if necessary)
            // Example: Show placeholder if no category is selected
            var selectedCategory = CategoryTreeView.SelectedItem as Category;
            if (selectedCategory == null)
            {
                MessageBox.Show("Please select a category from the TreeView.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LoadCategories()
        {
            // Load categories dynamically (mock example)
            _categories = new List<Category>
        {
            new Category { Name = "Electronics", SubCategories = new List<Category> { new Category { Name = "Mobile" }, new Category { Name = "Laptop" } } },
            new Category { Name = "Furniture", SubCategories = new List<Category> { new Category { Name = "Sofa" }, new Category { Name = "Table" } } },
            new Category { Name = "Stationery" }
        };

            CategoryTreeView.ItemsSource = _categories;
        }

        private void AddParentCategory_Click(object sender, RoutedEventArgs e)
        {
            var categoryName = PromptForName("Enter parent category name:");
            if (!string.IsNullOrEmpty(categoryName))
            {
                var index = _categories.Count + 1;
                var (abbr, id) = GenerateCategoryIdentifier(1, index);

                var newCategory = new Category
                {
                    Id = id,
                    Name = categoryName,
                    Abbreviation = abbr,
                    Level = 1
                };

                _categories.Add(newCategory);

                RefreshTreeView();
            }
        }


        private void AddSubCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryTreeView.SelectedItem is Category selectedCategory)
            {
                if (selectedCategory.Level >= 5)
                {
                    MessageBox.Show("You cannot add a subcategory beyond level 5.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var subCategoryName = PromptForName($"Enter name for subcategory under '{selectedCategory.Name}':");
                if (!string.IsNullOrEmpty(subCategoryName))
                {
                    var level = selectedCategory.Level + 1;
                    var index = selectedCategory.SubCategories.Count + 1;
                    var (abbr, id) = GenerateCategoryIdentifier(level, index);

                    var newSubCategory = new Category
                    {
                        Id = id,
                        Name = subCategoryName,
                        Abbreviation = abbr,
                        Level = level,
                        Parent = selectedCategory
                    };

                    selectedCategory.SubCategories.Add(newSubCategory);
                    RefreshTreeView();
                }
            }
            else
            {
                MessageBox.Show("Please select a category to add a subcategory.", "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private string PromptForName(string message)
        {
            return Microsoft.VisualBasic.Interaction.InputBox(message, "Input");
        }

        private void AddItem_Click(object sender, RoutedEventArgs e)
        {
            string itemName = ItemNameTextBox.Text.Trim();
            string itemDescription = ItemDescriptionTextBox.Text.Trim();
            string quantityText = QuantityTextBox.Text.Trim();
            string priceText = PriceTextBox.Text.Trim();
            var selectedCategory = CategoryTreeView.SelectedItem as Category;

            if (string.IsNullOrEmpty(itemName) || string.IsNullOrEmpty(quantityText) || string.IsNullOrEmpty(priceText) || selectedCategory == null)
            {
                MessageBox.Show("Please fill in all required fields.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(quantityText, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Quantity must be a positive number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!decimal.TryParse(priceText, out decimal price) || price < 0)
            {
                MessageBox.Show("Price must be a valid non-negative number.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            MessageBox.Show($"Item '{itemName}' added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Clear form
            ItemNameTextBox.Text = "";
            ItemDescriptionTextBox.Text = "";
            QuantityTextBox.Text = "";
            PriceTextBox.Text = "";
            //CategoryTreeView.SelectedItem = null;

            // Reset placeholders
            ToggleTextBoxPlaceholder(ItemNameTextBox, ItemNamePlaceholder);
            ToggleTextBoxPlaceholder(ItemDescriptionTextBox, ItemDescriptionPlaceholder);
            ToggleTextBoxPlaceholder(QuantityTextBox, QuantityPlaceholder);
            ToggleTextBoxPlaceholder(PriceTextBox, PricePlaceholder);
        }


        private void CategoryTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (_isRefreshingTreeView)
                return;

            var selectedCategory = CategoryTreeView.SelectedItem as Category;
            if (selectedCategory != null)
            {
                SelectedCategoryTextBlock.Text = $"Selected Category: {selectedCategory.DisplayName}";

                // Disable Add button if level is 5 or more
                AddSubCategoryButton.IsEnabled = selectedCategory.Level < 5;
            }
            else
            {
                SelectedCategoryTextBlock.Text = "Selected Category: None";
                AddSubCategoryButton.IsEnabled = false;

                // Show placeholder only when no category is selected
                ToggleTreeViewPlaceholder();
            }
        }


        private (string Abbreviation, string Id) GenerateCategoryIdentifier(int level, int index)
        {
            string prefix = level switch
            {
                1 => "PCG", // Parent Category Group
                2 => "SCG", // Sub Category Group
                3 => "CAT", // Category
                4 => "CHC", // Child Category
                5 => "SCC", // Sub Child Category
                _ => "LVL" + level
            };

            string id = $"{prefix}{index.ToString("D3")}";
            return (prefix, id);
        }

        private void RefreshTreeView()
        {
            _isRefreshingTreeView = true;

            // Clear and reset ItemsSource
            CategoryTreeView.ItemsSource = null;
            CategoryTreeView.ItemsSource = _categories;

            _isRefreshingTreeView = false;
        }

        private void CategorySearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            // Ensure CategorySearchBox is not null
            if (CategorySearchBox == null) return;

            string query = CategorySearchBox.Text.Trim().ToLower();

            if (string.IsNullOrWhiteSpace(query))
            {
                // Check if CategoryTreeView is null before assigning
                if (CategoryTreeView != null)
                {
                    CategoryTreeView.ItemsSource = _categories;
                }
            }
            else
            {
                // Ensure _categories list is not null
                if (_categories != null)
                {
                    var filtered = _categories
                        .Where(c => ContainsCategory(c, query))
                        .Select(c => FilterCategory(c, query))
                        .ToList();

                    // Check if CategoryTreeView is null before assigning
                    if (CategoryTreeView != null)
                    {
                        CategoryTreeView.ItemsSource = filtered;
                    }
                }
            }
        }


        private bool ContainsCategory(Category category, string query)
        {
            return category.Name.ToLower().Contains(query) ||
                   category.SubCategories.Any(sub => ContainsCategory(sub, query));
        }

        private Category FilterCategory(Category category, string query)
        {
            var matchingSubcategories = category.SubCategories
                .Where(sub => ContainsCategory(sub, query))
                .Select(sub => FilterCategory(sub, query))
                .ToList();

            return new Category
            {
                Id = category.Id,
                Name = category.Name,
                Abbreviation = category.Abbreviation,
                Level = category.Level,
                Parent = category.Parent,
                SubCategories = matchingSubcategories
            };
        }
    }

    public class Category
    {
        public string Id { get; set; } // e.g., PCG001, SCG002
        public string Name { get; set; }
        public string Abbreviation { get; set; } // e.g., PCG, SCG
        public int Level { get; set; } // 1 to 5
        public Category Parent { get; set; }
        public List<Category> SubCategories { get; set; } = new List<Category>();
        public string DisplayName => $"{Name} ({Abbreviation})";
    }


}
