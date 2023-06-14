using System;

namespace Kaktos.UserImmediateActions.Models
{
    public class ImmediateActionDataModel
    {
        public ImmediateActionDataModel(DateTimeOffset addedDate, AddPurpose purpose)
        {
            AddedDate = addedDate;
            Purpose = purpose;
        }

        /// <summary>
        /// Gets the date of adding this model.
        /// </summary>
        public DateTimeOffset AddedDate { get; }


        /// <summary>
        /// Gets the purpose of adding this model.
        /// </summary>
        public AddPurpose Purpose { get; }
    }
}