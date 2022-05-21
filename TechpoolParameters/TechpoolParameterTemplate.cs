using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace TechpoolUnleashed
{
    public partial class MainWindow
    {
        struct TechpoolParameterTemplate
        {
            public string Name;
            public double Parameter;

            public Grid Grid;
            public Label NameLabel;
            public Slider ParameterSlider;
            public TextBox ParameterDisplay;

            public TechpoolParameterTemplate(Grid template, Label nameLabel, Slider slider, TextBox parameterDisplay)
            {
                Grid = template;
                NameLabel = nameLabel;
                ParameterSlider = slider;
                ParameterDisplay = parameterDisplay;

                Name = (string)NameLabel.Content;
                Parameter = slider.Value;
            }

            public TechpoolParameterTemplate(TechpoolParameter techpoolParameter, TechpoolParameterTemplate template)
            {
                Name = techpoolParameter.Name;
                Parameter = techpoolParameter.Parameter;

                Grid = new Grid();
                this.Grid.Width = template.Grid.Width;
                this.Grid.Height = template.Grid.Height;
                this.Grid.LayoutTransform = template.Grid.LayoutTransform;
                this.Grid.HorizontalAlignment = template.Grid.HorizontalAlignment;
                this.Grid.VerticalAlignment = template.Grid.VerticalAlignment;
                //Thickness gridMargin = template.Grid.Margin;
                //gridMargin.Top = MainWindow.Instance.ParamTemplateHeight;
                //this.Grid.Margin = gridMargin;
                this.Grid.Margin = template.Grid.Margin;
                //this.Grid.Margin.Top = MainWindow.Instance.ParamTemplateHeight;

                #region Widget Initialization
                //Name Label
                NameLabel = new Label();
                Grid.Children.Add(NameLabel);
                NameLabel.Content = (object)Name;
                NameLabel.Foreground = template.NameLabel.Foreground;
                NameLabel.Background = template.NameLabel.Background;
                NameLabel.FontFamily = template.NameLabel.FontFamily;
                NameLabel.FontSize = template.NameLabel.FontSize;
                NameLabel.FontStyle = template.NameLabel.FontStyle;
                NameLabel.FontStretch = template.NameLabel.FontStretch;
                NameLabel.FontWeight = template.NameLabel.FontWeight;
                NameLabel.Width = template.NameLabel.Width;
                NameLabel.Height = template.NameLabel.Height;
                NameLabel.LayoutTransform = template.NameLabel.LayoutTransform;
                NameLabel.HorizontalAlignment = template.NameLabel.HorizontalAlignment;
                NameLabel.VerticalAlignment = template.NameLabel.VerticalAlignment;
                NameLabel.HorizontalContentAlignment = template.NameLabel.HorizontalContentAlignment;
                NameLabel.VerticalContentAlignment = template.NameLabel.VerticalContentAlignment;
                NameLabel.Padding = template.NameLabel.Padding;
                NameLabel.Margin = template.NameLabel.Margin;

                //Slider
                ParameterSlider = new Slider();
                Grid.Children.Add(ParameterSlider);
                ParameterSlider.Minimum = template.ParameterSlider.Minimum;
                ParameterSlider.Maximum = template.ParameterSlider.Maximum;
                ParameterSlider.Width = template.ParameterSlider.Width;
                ParameterSlider.Height = template.ParameterSlider.Height;
                ParameterSlider.LayoutTransform = template.ParameterSlider.LayoutTransform;
                ParameterSlider.HorizontalAlignment = template.ParameterSlider.HorizontalAlignment;
                ParameterSlider.VerticalAlignment = template.ParameterSlider.VerticalAlignment;
                ParameterSlider.HorizontalContentAlignment = template.ParameterSlider.HorizontalContentAlignment;
                ParameterSlider.VerticalContentAlignment = template.ParameterSlider.VerticalContentAlignment;
                ParameterSlider.Padding = template.ParameterSlider.Padding;
                ParameterSlider.Margin = template.ParameterSlider.Margin;

                //Slider display
                ParameterDisplay = new TextBox();
                Grid.Children.Add(ParameterDisplay);
                ParameterDisplay.Text = Parameter.ToString();
                ParameterDisplay.Foreground = template.ParameterDisplay.Foreground;
                ParameterDisplay.Background = template.ParameterDisplay.Background;
                ParameterDisplay.FontFamily = template.ParameterDisplay.FontFamily;
                ParameterDisplay.FontSize = template.ParameterDisplay.FontSize;
                ParameterDisplay.FontStyle = template.ParameterDisplay.FontStyle;
                ParameterDisplay.FontStretch = template.ParameterDisplay.FontStretch;
                ParameterDisplay.FontWeight = template.ParameterDisplay.FontWeight;
                ParameterDisplay.Width = template.ParameterDisplay.Width;
                ParameterDisplay.Height = template.ParameterDisplay.Height;
                ParameterDisplay.LayoutTransform = template.ParameterDisplay.LayoutTransform;
                ParameterDisplay.HorizontalAlignment = template.ParameterDisplay.HorizontalAlignment;
                ParameterDisplay.VerticalAlignment = template.ParameterDisplay.VerticalAlignment;
                ParameterDisplay.HorizontalContentAlignment = template.ParameterDisplay.HorizontalContentAlignment;
                ParameterDisplay.VerticalContentAlignment = template.ParameterDisplay.VerticalContentAlignment;
                ParameterDisplay.Padding = template.ParameterDisplay.Padding;
                ParameterDisplay.Margin = template.ParameterDisplay.Margin;
                #endregion

                #region Events

                var slider = ParameterSlider;
                var sliderLabel = ParameterDisplay;
                var paramTitle = NameLabel;

                slider.Value = Parameter;
                slider.ValueChanged += (object sender, RoutedPropertyChangedEventArgs<double> e) =>
                {
                    sliderLabel.Text = Math.Round(e.NewValue).ToString();
                    _newParameters[(string)paramTitle.Content] = Math.Round(e.NewValue);
                    MainWindow.Instance.CheckUpdatedValues();
                };


                sliderLabel.KeyDown += (object sender, KeyEventArgs e) =>
                {
                    if (e.Key == Key.Enter)
                    {
                        try
                        {
                            if (sliderLabel.Text == "")
                            {
                                slider.Value = 0;
                                sliderLabel.Text = "0";
                                _newParameters[(string)paramTitle.Content] = 0;
                                MainWindow.Instance.CheckUpdatedValues();
                            }
                            else
                            {
                                slider.Value = Math.Min(MainWindow.Instance._maxTechPool, Math.Max(MainWindow.Instance._minTechPool, double.Parse(sliderLabel.Text)));
                                sliderLabel.Text = slider.Value.ToString();
                                _newParameters[(string)paramTitle.Content] = Math.Min(MainWindow.Instance._maxTechPool, Math.Max(MainWindow.Instance._minTechPool, double.Parse(sliderLabel.Text)));
                                MainWindow.Instance.CheckUpdatedValues();
                            }
                        }
                        catch { slider.Value = 0; sliderLabel.Text = "0"; }
                    }
                };
                sliderLabel.LostFocus += (object sender, RoutedEventArgs e) =>
                {

                    try
                    {
                        if (sliderLabel.Text == "")
                        {
                            slider.Value = 0;
                            sliderLabel.Text = "0";
                        }
                        else
                        {
                            slider.Value = Math.Min(MainWindow.Instance._maxTechPool, Math.Max(MainWindow.Instance._minTechPool, double.Parse(sliderLabel.Text)));
                            sliderLabel.Text = slider.Value.ToString();
                        }
                    }
                    catch { slider.Value = 0; sliderLabel.Text = "0"; }

                };
                #endregion
            }
        }
    }
}
