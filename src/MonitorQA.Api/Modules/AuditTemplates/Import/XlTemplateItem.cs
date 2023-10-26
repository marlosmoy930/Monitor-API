using ClosedXML.Excel;
using MonitorQA.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MonitorQA.Api.Modules.AuditTemplates.Import
{
    public class XlTemplateItem
    {
        const string isConditionalHeaderName = "IsConditional";
        const string isFailedConditionHeaderName = "IsFailedCondition";
        const string passedPointsHeaderName = "PassedPoints";
        const string failedPointsHeaderName = "FailedPoints";
        const string conditionalValue = "1";

        public readonly bool HasData;
        private readonly IXLCell _cell;

        public bool IsConditional { get; private set; }
        public bool IsFailedCondition { get; private set; }

        private XlTemplateItem(IXLCell cell)
        {
            HasData = true;
            this._cell = cell;
        }

        private XlTemplateItem()
        {
            HasData = false;
        }

        public static XlTemplateItem Create(IXLCell cell)
        {
            var value = cell.GetString();
            if (!string.IsNullOrEmpty(value))
            {
                return new XlTemplateItem(cell);
            }
            return new XlTemplateItem();
        }

        public TemplateItem GetTemplateItem(Guid templateId)
        {
            var text = _cell
                .GetString()
                .Trim();

            IsConditional = GetIsConditional(isConditionalHeaderName);
            IsFailedCondition = GetIsConditional(isFailedConditionHeaderName);

            var itemType = IsConditional ? ItemType.ConditionalItem : ItemType.Item;

            var templateItemId = Guid.NewGuid();
            var templateItem = new TemplateItem
            {
                Id = templateItemId,
                AnswerType = AnswerType.Buttons,
                ItemType = itemType,
                Text = text,
                Answers = CreateButtonAnswers(templateItemId),
                TemplateId = templateId,
                Children = new List<TemplateItem>(),
            };

            if (IsConditional)
            {
                var failedAnswer = templateItem.Answers.Single(a => a.Name == TemplateItemAnswer.Failed.Name);
                templateItem.AddConditionItem(failedAnswer.Name);
            }

            var passedPoints = GetPoints(passedPointsHeaderName);
            if (passedPoints.HasValue)
            {
                var passedAnswer = templateItem.Answers.Single(a => a.Name == TemplateItemAnswer.Passed.Name);
                passedAnswer.Points = passedPoints;
            }

            var failedPoints = GetPoints(failedPointsHeaderName);
            if (failedPoints.HasValue)
            {
                var failedAnswer = templateItem.Answers.Single(a => a.Name == TemplateItemAnswer.Failed.Name);
                failedAnswer.Points = failedPoints;
            }

            return templateItem;
        }

        public bool GetIsConditional(string headerName)
        {
            var cell = GetCellByHeaderName(headerName);
            if (cell == null) return false;

            return conditionalValue.Equals(cell.GetString(), StringComparison.OrdinalIgnoreCase);
        }

        public static List<TemplateItemAnswer> CreateButtonAnswers(Guid templateItemId)
        {
            var list = new List<TemplateItemAnswer>
            {
                TemplateItemAnswer.Passed.Create(templateItemId),
                TemplateItemAnswer.Failed.Create(templateItemId),
            };
            return list;
        }

        private string Name => $"{_cell.Address} {_cell.GetString()}";

        private int? GetPoints(string headerName)
        {
            var cell = GetCellByHeaderName(headerName);
            if (cell == null) return null;

            if (cell.TryGetValue<double>(out var points))
            {
                return (int)points;
            }
            return null;
        }

        private IXLCell? GetCellByHeaderName(string headerName)
        {
            var worksheet = _cell.Worksheet;
            var headerCell = worksheet
                .Row(WorksheetWithTemplateToImport.HeaderRowNumber)
                .CellsUsed()
                .SingleOrDefault(c => headerName.Equals(c.GetString(), StringComparison.OrdinalIgnoreCase));

            if (headerCell == null) return null;

            var rowNumber = _cell.WorksheetRow().RowNumber();
            var colNumber = headerCell.WorksheetColumn().ColumnNumber();
            var cell = worksheet.Cell(rowNumber, colNumber);
            return cell;
        }
    }
}
