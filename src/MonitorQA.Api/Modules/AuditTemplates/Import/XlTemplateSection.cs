using ClosedXML.Excel;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditTemplates.Import
{
    public class XlTemplateSection
    {
        const string emptySectionName = "Empty section";

        private readonly IXLCell _cell;
        private readonly List<XlTemplateSection> _xlSubsections;
        private readonly List<XlTemplateItem> _items;

        private XlTemplateSection(
            IXLCell xLCell,
            List<XlTemplateSection> xlSubsections,
            List<XlTemplateItem> items)
        {
            this._cell = xLCell;
            this._xlSubsections = xlSubsections;
            this._items = items;
        }

        public static TemplateItem CreateRoot(IXLCell startCell, Guid templateId)
        {
            var xlSubsection = GetXlSubsections(startCell, null);
            var children = xlSubsection
                .Select( (s, i) => s.GetSection(i, templateId))
                .ToList();

            var root = new TemplateItem
            {
                Id = templateId,
                ItemType = ItemType.Root,
                Text = null,
                Index = 0,
                TemplateId = templateId,
                Children = children,
            };
            return root;
        }

        private TemplateItem GetSection(int index, Guid templateId)
        {
            var children = new List<TemplateItem>();
            children.AddRange(GetSubsectionItems(templateId));
            children.AddRange(GetTemplateItems(templateId));

            var section = new TemplateItem
            {
                Id = Guid.NewGuid(),
                AnswerType = AnswerType.Buttons,
                ItemType = ItemType.Section,
                Text = GetSectionText(),
                Index = index,
                TemplateId = templateId,
                Children = children
            };
            return section;
        }

        private static XlTemplateSection Create(IXLCell cell, IXLCell? nextCell)
        {
            var items = new List<XlTemplateItem>();
            var subsections = new List<XlTemplateSection>();

            var cellRight = cell.CellRight();
            var nextCellRight = nextCell?.CellRight();

            var isItemColumn = GetIsItemColumn(cellRight);
            if (isItemColumn)
            {
                items = GetXlItems(cellRight, nextCellRight);
            }
            else
            {
                subsections = GetXlSubsections(cellRight, nextCellRight);
            }

            var section = new XlTemplateSection(cell, subsections, items);
            return section;
        }

        private List<TemplateItem> GetSubsectionItems(Guid templateId)
        {
            if (_xlSubsections == null) return new List<TemplateItem>();

            var subsections = _xlSubsections
                .Select((s, i) => s.GetSection(i, templateId))
                .ToList();
            return subsections;
        }

        private List<TemplateItem> GetTemplateItems(Guid templateId)
        {
            if (_items == null) return new List<TemplateItem>();

            var templateItems = new List<TemplateItem>();
            TemplateItem? conditionItem = null;
            foreach (var item in _items)
            {
                var templateItem = item.GetTemplateItem(templateId);

                if (item.IsConditional)
                {
                    conditionItem = templateItem.GetConditionItem();
                    templateItems.Add(templateItem);
                }
                else if (item.IsFailedCondition)
                {
                    conditionItem.Children.Add(templateItem);
                }
                else
                {
                    templateItems.Add(templateItem);
                }
            }

            SetIdexes(templateItems);

            return templateItems;
        }

        private string Name => $"{_cell.Address} {GetSectionText()}";

        private string GetSectionText() => _cell.GetString()?.Trim() ?? emptySectionName;

        private void SetIdexes(List<TemplateItem> items)
        {
            var index = 0;
            foreach (var item in items)
            {
                item.Index = index++;
                if (item.ItemType == ItemType.ConditionalItem
                    || item.ItemType == ItemType.Condition)
                {
                    var children = item.Children.ToList();
                    SetIdexes(children);
                }
            }
        }

        private static List<XlTemplateSection> GetXlSubsections(IXLCell cell, IXLCell? nextParentCell)
        {
            var rowNumber = cell.WorksheetRow().RowNumber();
            var nextCellRowNumber = nextParentCell?.WorksheetRow().RowNumber();

            var cells = cell
                .WorksheetColumn()
                .CellsUsed()
                .Where(c => IsCellBetweenRows(c, rowNumber, nextCellRowNumber))
                .ToList();

            var nextCells = cells
                .Skip(1)
                .ToList();
            nextCells.Add(nextParentCell);

            var sectionCells = cells
                .Zip(nextCells, (c, next) => Create(c, next))
                .ToList();
            return sectionCells;
        }

        private static List<XlTemplateItem> GetXlItems(IXLCell cell, IXLCell? nextCell)
        {
            var rowNumber = cell.WorksheetRow().RowNumber();
            var columnNumber = cell
                .WorksheetColumn()
                .ColumnNumber();
            var nextCellRowNumber = nextCell?.WorksheetRow().RowNumber();
            var worksheet = cell.Worksheet;

            var items = new List<XlTemplateItem>();
            while (AddXlItem(items, worksheet, rowNumber, columnNumber, nextCellRowNumber))
            {
                rowNumber++;
            }

            return items;
        }

        private static bool GetIsItemColumn(IXLCell cell)
        {
            var header = cell
                .WorksheetColumn()
                .Cell(WorksheetWithTemplateToImport.HeaderRowNumber)
                .GetString();

            if (string.IsNullOrEmpty(header)) return false;

            var headerUpper = header.ToUpperInvariant();

            return headerUpper.Contains("QUESTION", StringComparison.OrdinalIgnoreCase)
                || headerUpper.Contains("ITEM", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsCellBetweenRows(IXLCell cell, int firstRowNumber, int? lastRowNumber)
        {
            var currentRowNumber = cell.WorksheetRow().RowNumber();
            if (currentRowNumber < firstRowNumber) return false;
            
            if (currentRowNumber == firstRowNumber) return true;

            return lastRowNumber.HasValue 
                ? currentRowNumber < lastRowNumber.Value
                : true;
        }

        private static bool AddXlItem(List<XlTemplateItem> items, IXLWorksheet worksheet, int rowNumber, int columnNumber, int? nextCellRowNumber)
        {
            var itemCell = worksheet.Cell(rowNumber, columnNumber);
            var item = XlTemplateItem.Create(itemCell);
            var canAddItem = CanAddXlItem(item, rowNumber, nextCellRowNumber);
            if (canAddItem)
            {
                items.Add(item);
            }
            return canAddItem;
        }

        private static bool CanAddXlItem(XlTemplateItem item, int rowNumber, int? nextCellRowNumber)
        {
            if (nextCellRowNumber.HasValue)
            {
                return rowNumber < nextCellRowNumber.Value;
            }

            return item.HasData;
        }
    }
}
