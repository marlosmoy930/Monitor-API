using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using MonitorQA.Data.Entities;
using Newtonsoft.Json;

namespace MonitorQA.Api.Modules.AuditTemplates.Models.AnswerTypes
{
    public abstract class AnswerDataBase
    {
        public static AnswerDataBase Create(AnswerType answerType, TemplateItemAnswer answer)
        {
            switch (answerType)
            {
                case AnswerType.Buttons:
                    return AnswerButton.Create(answer);

                case AnswerType.YesNoButtons:
                    return YesNoButtonsAnswer.Create(answer);

                case AnswerType.Numeric:
                    return NumericAnswer.Create(answer);

                case AnswerType.Checklist:
                    return ChecklistAnswer.Create(answer);

                case AnswerType.Text:
                    return TextAnswer.Create(answer);
            }

            throw new InvalidEnumArgumentException("answerType");
        }

        public static List<AnswerDataBase> Create(AnswerType answerType, object answerData)
        {
            switch (answerType)
            {
                case AnswerType.Buttons:
                    return ConvertJsonElementObjectToType<List<AnswerButton>>(answerData).ToList<AnswerDataBase>();

                case AnswerType.YesNoButtons:
                    return ConvertJsonElementObjectToType<List<YesNoButtonsAnswer>>(answerData)
                        .ToList<AnswerDataBase>();

                case AnswerType.Numeric:
                    return ConvertJsonElementObjectToType<List<NumericAnswer>>(answerData).ToList<AnswerDataBase>();

                case AnswerType.Checklist:
                    return ConvertJsonElementObjectToType<List<ChecklistAnswer>>(answerData).ToList<AnswerDataBase>();

                case AnswerType.Text:
                    return new List<AnswerDataBase> { ConvertJsonElementObjectToType<TextAnswer>(answerData) };
            }

            throw new InvalidEnumArgumentException("answerType");
        }


        public abstract TemplateItemAnswer GetTemplateItemAnswerEntity(Data.Entities.TemplateItem item, int index);

        private static T ConvertJsonElementObjectToType<T>(object? modelProperty)
        {
            if (!(modelProperty is JsonElement jsonElement))
            {
                throw new ArgumentException("modelProperty is not a JsonElement");
            }

            var json = jsonElement.GetRawText();
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}