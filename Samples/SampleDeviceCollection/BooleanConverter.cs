//----------------------------------------------------------------------------------------------
// <copyright file="BooleanConverter.cs" company="Microsoft Corporation">
//     Licensed under the MIT License. See LICENSE.TXT in the project root license information.
// </copyright>
//----------------------------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace SampleDeviceCollection
{
    //-------------------------------------------------------------------
    //  Boolean Converter
    //-------------------------------------------------------------------
    #region Boolean Converter
    /// <summary>
    ///     Template allows for easy creation of a value converter for bools
    /// </summary>
    /// <typeparam name="T">Type to convert back and forth to boolean</typeparam>
    /// <remarks>
    ///     See BooleanToVisibilityConverter and BooleanToBrushConverter (below) and usage in Generic.xaml
    /// </remarks>
    public class BooleanConverter<T> : IValueConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanConverter{T}" /> class.
        /// </summary>
        /// <param name="trueValue">The value of type T that represents true</param>
        /// <param name="falseValue">The value of type T that represents false</param>
        public BooleanConverter(T trueValue, T falseValue)
        {
            this.True = trueValue;
            this.False = falseValue;
        }

        /// <summary>
        /// Gets or sets the value that represents true
        /// </summary>
        public T True { get; set; }

        /// <summary>
        /// Gets or sets the value that represetns false
        /// </summary>
        public T False { get; set; }

        /// <summary>
        /// Convert an object of type T to boolean
        /// </summary>
        /// <param name="value">Object of type T to convert</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>Object of boolean value</returns>
        public virtual object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is bool && ((bool)value) ? this.True : this.False;
        }

        /// <summary>
        /// Convert a boolean value to an object of type T
        /// </summary>
        /// <param name="value">The boolean value to convert</param>
        /// <param name="targetType">The parameter is not used.</param>
        /// <param name="parameter">The parameter is not used.</param>
        /// <param name="culture">The parameter is not used.</param>
        /// <returns>Object of type T</returns>
        public virtual object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value is T && EqualityComparer<T>.Default.Equals((T)value, this.True);
        }
    }

    /// <summary>
    /// Converter between a boolean and visibility value
    /// </summary>
    [type: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Small classes are all instances of the same generic and are better organized in a single file.")]
    public sealed class BooleanToVisibilityConverter : BooleanConverter<Visibility>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToVisibilityConverter" /> class.
        /// </summary>
        public BooleanToVisibilityConverter() :
            base(Visibility.Visible, Visibility.Hidden)
        {
        }
    }

    /// <summary>
    /// Converter between a boolean and either "http" or "https"
    /// </summary>
    [type: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1402:FileMayOnlyContainASingleClass", Justification = "Small classes are all instances of the same generic and are better organized in a single file.")]
    public sealed class BooleanToHttpsConverter : BooleanConverter<string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanToHttpsConverter" /> class.
        /// </summary>
        public BooleanToHttpsConverter() :
            base("https", "http")
        {
        }
    }
    #endregion // Boolean Converter
}
