using System;

namespace Kaktos.UserImmediateActions.Models
{
    public class ImmediateActionDataModel
    {
        /// <summary>
        /// Constructs new instance of <see cref="ImmediateActionDataModel"/>
        /// </summary>
        /// <param name="addedDate">Added date of this model, typically its current date time</param>
        /// <param name="purpose">Purpose or what operation needs to be done on user</param>
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