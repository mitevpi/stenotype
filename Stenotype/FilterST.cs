using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace Stenotype
{
    /// <summary>
    /// Wrapper class for a Revit Filter object.
    /// </summary>
    /// <remarks>
    /// ADD.
    /// </remarks>
    public class FilterST
    {
        /// <summary>
        /// The name of the Filter Element.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The categories associated with/controlled by this filter as list of Element ID.
        /// </summary>
        public ICollection<ElementId> Categories { get; }

        /// <summary>
        /// A list of rules associated with the Filter element.
        /// </summary>
        public List<FilterRule> FilterRules { get; }

        /// <summary>
        /// Initialize from a Filter object.
        /// </summary>
        /// <param name="filterElement"></param>
        public FilterST(ParameterFilterElement filterElement)
        {
            Name = filterElement.Name;
            Categories = filterElement.GetCategories();
            FilterRules = filterElement.GetRules().ToList();
        }

        /// <summary>
        /// Gets all View Filter Elements.
        /// </summary>
        /// <param name="doc">The Revit Document object.</param>
        /// <returns>A list of all the View Filter Elements in the Document.</returns>
        public static List<FilterST> GetViewFilters(Document doc)
        {
            // Collect all filters in the document
            ICollection<Element> collectorElements = new FilteredElementCollector(doc).OfClass(typeof(ParameterFilterElement)).WhereElementIsNotElementType().ToElements();

            // Create classes for easy access of items via properties
            List<ParameterFilterElement> filterElements = collectorElements.Select(fe => (ParameterFilterElement)fe).ToList();
            List<FilterST> documentFilters = filterElements.Select(fe => new FilterST(fe)).ToList();

            return documentFilters;
        }

        /// <summary>
        /// Creates a filter in a Revit Document from a custom FilterST class instance.
        /// </summary>
        /// <param name="doc">The Revit Document Object.</param>
        /// <param name="fRaw">A FilterST object.</param>
        public static void CreateViewFilterFromObject(Document doc, FilterST fRaw)
        {
            string filterName = fRaw.Name;
            ICollection<ElementId> categories = fRaw.Categories;
            List<FilterRule> filterRules = fRaw.FilterRules;

            try
            {
                using (Transaction t = new Transaction(doc, "Add View Filter"))
                {
                    t.Start();

                    ParameterFilterElement parameterFilterElement = ParameterFilterElement.Create(doc, filterName, categories);
                    parameterFilterElement.SetRules(filterRules);

                    t.Commit();
                    t.Dispose();
                }
            }
            catch (Exception filterException)
            {
                MessageBox.Show(filterException.Message);
            }
        }

        /// <summary>
        /// Attempts to transfer View Filter Elements between two active documents. If the associated Parameters of each filter are built-in
        /// or shared between both documents, the operation will succeed. Otherwise, the rules will not be able to be applied to the target document.
        /// </summary>
        /// <param name="docBase">The Revit Document object to take Filters from.</param>
        /// <param name="docTarget">The Revit Document object to transfer Filters to.</param>
        public static void TransferViewFilters(Document docBase, Document docTarget)
        {
            List<FilterST> documentFiltersToTransfer = GetViewFilters(docBase);

            foreach (FilterST fRaw in documentFiltersToTransfer)
            {
                CreateViewFilterFromObject(docTarget, fRaw);
            }
        }
    }
}
