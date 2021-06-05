using System;

namespace UserImmediateActions.Models
{
    public class ImmediateActionDataModel
    {
        public ImmediateActionDataModel(DateTime addedDate, AddPurpose purpose)
        {
            AddedDate = addedDate;
            Purpose = purpose;
        }

        /// <summary>
        /// Gets the date of adding this model.
        /// </summary>
        public DateTime AddedDate { get; }


        /// <summary>
        /// Gets the purpose of adding this model.
        /// </summary>
        public AddPurpose Purpose { get; }
    }
}