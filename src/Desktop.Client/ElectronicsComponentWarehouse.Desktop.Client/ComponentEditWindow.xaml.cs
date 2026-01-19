using ElectronicsComponentWarehouse.Desktop.Client.Models.Categories;
using ElectronicsComponentWarehouse.Desktop.Client.Models.Components;
using ElectronicsComponentWarehouse.Desktop.Client.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicsComponentWarehouse.Desktop.Client
{
    public partial class ComponentEditWindow : Window
    {
        private readonly IComponentService _componentService;
        private readonly ICategoryService _categoryService;
        private readonly bool _isAdmin;
        private readonly IServiceProvider _serviceProvider;

        private ComponentModel? _component;
        private List<CategoryModel> _categories = new();

        public ComponentEditWindow(
            IComponentService componentService,
            ICategoryService categoryService,
            bool isAdmin,
            IServiceProvider serviceProvider,
            ComponentModel? component = null)
        {
            InitializeComponent();

            _componentService = componentService ?? throw new ArgumentNullException(nameof(componentService));
            _categoryService = categoryService ?? throw new ArgumentNullException(nameof(categoryService));
            _isAdmin = isAdmin;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _component = component;

            Loaded += ComponentEditWindow_Loaded;
        }

        private async void ComponentEditWindow_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await LoadCategoriesAsync();
                InitializeForm();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading component edit window");
                MessageBox.Show($"Error loading form: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                _categories = categories.ToList();

                CategoryComboBox.ItemsSource = _categories;
                CategoryComboBox.DisplayMemberPath = "Name";
                CategoryComboBox.SelectedValuePath = "Id";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error loading categories");
                throw;
            }
        }

        private void InitializeForm()
        {
            if (_component != null)
            {
                // Edit mode
                TitleText.Text = "Edit Component";
                SubtitleText.Text = $"Editing: {_component.Name}";

                NameTextBox.Text = _component.Name;
                DescriptionTextBox.Text = _component.Description;
                QuantityTextBox.Text = _component.StockQuantity.ToString();
                MinStockTextBox.Text = _component.MinimumStockLevel.ToString();
                LocationTextBox.Text = _component.StorageCellNumber;
                ManufacturerTextBox.Text = _component.Manufacturer;
                ModelNumberTextBox.Text = _component.ModelNumber;
                PriceTextBox.Text = _component.UnitPrice?.ToString();
                DatasheetTextBox.Text = _component.DatasheetLink;

                // Select category
                CategoryComboBox.SelectedValue = _component.CategoryId;

                // Set field availability based on admin status
                if (!_isAdmin)
                {
                    NameTextBox.IsEnabled = false;
                    DescriptionTextBox.IsEnabled = false;
                    MinStockTextBox.IsEnabled = false;
                    LocationTextBox.IsEnabled = false;
                    ManufacturerTextBox.IsEnabled = false;
                    ModelNumberTextBox.IsEnabled = false;
                    PriceTextBox.IsEnabled = false;
                    CategoryComboBox.IsEnabled = false;
                }
            }
            else
            {
                // Add mode
                TitleText.Text = "Add Component";
                SubtitleText.Text = "Add new component to inventory";

                // Only admin can add components
                if (!_isAdmin)
                {
                    MessageBox.Show("Only administrators can add new components",
                        "Permission Denied", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Close();
                }
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateForm())
            {
                return;
            }

            try
            {
                SaveButton.IsEnabled = false;

                var component = _component ?? new ComponentModel();

                // Update component data
                component.Name = NameTextBox.Text.Trim();
                component.Description = DescriptionTextBox.Text.Trim();
                component.StockQuantity = int.Parse(QuantityTextBox.Text);
                component.MinimumStockLevel = int.Parse(MinStockTextBox.Text);
                component.StorageCellNumber = LocationTextBox.Text.Trim();
                component.Manufacturer = ManufacturerTextBox.Text.Trim();
                component.ModelNumber = ModelNumberTextBox.Text.Trim();
                component.DatasheetLink = DatasheetTextBox.Text.Trim();

                if (decimal.TryParse(PriceTextBox.Text, out var price))
                {
                    component.UnitPrice = price;
                }

                if (CategoryComboBox.SelectedValue is int categoryId)
                {
                    component.CategoryId = categoryId;
                }

                // Save component
                ComponentModel? savedComponent;

                if (_component != null)
                {
                    // Update existing component
                    savedComponent = await _componentService.UpdateComponentAsync(component);
                }
                else
                {
                    // Create new component
                    savedComponent = await _componentService.CreateComponentAsync(component);
                }

                if (savedComponent != null)
                {
                    DialogResult = true;
                    Close();
                }
                else
                {
                    MessageBox.Show("Failed to save component", "Error",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error saving component");
                MessageBox.Show($"Error saving component: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                SaveButton.IsEnabled = true;
            }
        }

        private bool ValidateForm()
        {
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Please enter component name", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return false;
            }

            if (!int.TryParse(QuantityTextBox.Text, out var quantity) || quantity < 0)
            {
                MessageBox.Show("Please enter a valid quantity (non-negative number)",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                QuantityTextBox.Focus();
                return false;
            }

            if (!int.TryParse(MinStockTextBox.Text, out var minStock) || minStock < 0)
            {
                MessageBox.Show("Please enter a valid minimum stock level (non-negative number)",
                    "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                MinStockTextBox.Focus();
                return false;
            }

            if (string.IsNullOrWhiteSpace(LocationTextBox.Text))
            {
                MessageBox.Show("Please enter storage location", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                LocationTextBox.Focus();
                return false;
            }

            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Please select a category", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(PriceTextBox.Text))
            {
                if (!decimal.TryParse(PriceTextBox.Text, out var price) || price < 0)
                {
                    MessageBox.Show("Please enter a valid price (non-negative number)",
                        "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                    PriceTextBox.Focus();
                    return false;
                }
            }

            return true;
        }
    }
}