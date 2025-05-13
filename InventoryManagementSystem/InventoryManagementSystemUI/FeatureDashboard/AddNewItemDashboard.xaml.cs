using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using InventoryAppDataAccessLayer.Data;
using InventoryAppDataAccessLayer.Repositories.RepoInterfaces;
using InventoryAppDomainLayer.DataModels.HomeDashboardModels;
using InventoryAppServicesLayer.ServiceImplementations;
using InventoryAppServicesLayer.ServiceInterfaces;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryManagementSystemUI.FeatureDashboard
{
    /// <summary>
    /// Interaction logic for AddNewItemDashboard.xaml
    /// </summary>
    public partial class AddNewItemDashboard : UserControl
    {
        // List to hold categories
        private readonly ObservableCollection<Category> _categories = new();
        public ObservableCollection<Category> FilteredCategories => _categories;

        // The property bound to the selected category in the TreeView
        private Category SelectedCategory;
        private bool _isRefreshingTreeView = false;
        private readonly IAddItemCategoryService _categoryService;
        private bool _isLoadingCategories = false;
        public AddNewItemDashboard()
        {
            InitializeComponent();
            _categoryService = App.ServiceProvider.GetRequiredService<IAddItemCategoryService>();

            this.DataContext = this; // important for binding to work!

            HookTextBoxEvents(ItemNameTextBox, ItemNamePlaceholder);
            HookTextBoxEvents(ItemDescriptionTextBox, ItemDescriptionPlaceholder);
            HookTextBoxEvents(QuantityTextBox, QuantityPlaceholder);
            HookTextBoxEvents(PriceTextBox, PricePlaceholder);

            _ = LoadCategoriesAsync();
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

        private async Task LoadCategoriesAsync()
        {
            // Example: Loading categories into FilteredCategories
            var allCategories = await _categoryService.GetAllCategoriesAsync();
            FilteredCategories.Clear();
            foreach (var category in allCategories)
            {
                FilteredCategories.Add(category);
            }
        }


        private async void AddParentCategory_Click(object sender, RoutedEventArgs e)
        {
            var name = PromptForName("Enter parent category name:");
            if (!string.IsNullOrEmpty(name))
            {
                await _categoryService.AddParentCategoryAsync(name);
                RefreshTreeView();
            }
        }


        private async void AddSubCategory_Click(object sender, RoutedEventArgs e)
        {
            if (CategoryTreeView.SelectedItem is Category selectedCategory)
            {
                if (selectedCategory.Level >= 5)
                {
                    MessageBox.Show("You cannot add a subcategory beyond level 5.", "Limit Reached", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var subCategoryName = PromptForName($"Enter name for subcategory under '{selectedCategory.Name}':");
                if (!string.IsNullOrWhiteSpace(subCategoryName))
                {
                    try
                    {
                        // Save subcategory to the database
                        await _categoryService.AddSubCategoryAsync(selectedCategory.CategoryId, subCategoryName);

                        // Option 1: (Recommended) Reload the entire category tree
                        await LoadCategoriesAsync();
                        //RefreshTreeView();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to add subcategory: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
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

            // Reset placeholders
            ToggleTextBoxPlaceholder(ItemNameTextBox, ItemNamePlaceholder);
            ToggleTextBoxPlaceholder(ItemDescriptionTextBox, ItemDescriptionPlaceholder);
            ToggleTextBoxPlaceholder(QuantityTextBox, QuantityPlaceholder);
            ToggleTextBoxPlaceholder(PriceTextBox, PricePlaceholder);

            // Re-validate form state (disable Add button again)
            ValidateFormInputs();
        }


        private void InputField_TextChanged(object sender, TextChangedEventArgs e)
        {
            ValidateFormInputs();
        }
        private void ValidateFormInputs()
        {
            bool isValid =
                !string.IsNullOrWhiteSpace(ItemNameTextBox.Text) &&
                !string.IsNullOrWhiteSpace(QuantityTextBox.Text) &&
                int.TryParse(QuantityTextBox.Text, out int quantity) && quantity > 0 &&
                !string.IsNullOrWhiteSpace(PriceTextBox.Text) &&
                decimal.TryParse(PriceTextBox.Text, out decimal price) && price >= 0 &&
                CategoryTreeView.SelectedItem is Category;

            AddItemButton.IsEnabled = isValid;
        }


        private async void RefreshTreeView()
        {
            _isRefreshingTreeView = true;

            try
            {
                // Reuse LoadCategoriesAsync to handle the data refresh and binding
                await LoadCategoriesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh categories: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
                CategoryId = category.CategoryId,
                Name = category.Name,
                Abbreviation = category.Abbreviation,
                Level = category.Level,
                ParentCategoryId = category.ParentCategoryId,
                Parent = category.Parent,
                SubCategories = matchingSubcategories
            };
        }
        private void CategoryTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {

            // Get the selected category from TreeView
            if (e.NewValue is Category selectedCategory)
            {
                // Update the TextBlock with the selected category's ID and Name
                SelectedCategory = selectedCategory;
                SelectedCategoryTextBlock.Text = $"Selected Category: {selectedCategory.DisplayName} (Category ID: {selectedCategory.CategoryId})";
            }
        }
        private void EditCategory_Click(object sender, RoutedEventArgs e)
        {
        
        }

        private void DeleteCategory_Click(object sender, RoutedEventArgs e)
        {
          
        }
    }
}
