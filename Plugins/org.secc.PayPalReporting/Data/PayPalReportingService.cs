﻿
namespace org.secc.PayPalReporting.Data
{
    class PayPalReportingService<T> : Rock.Data.Service<T> where T : Rock.Data.Entity<T>, new()
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentationService{T}"/> class.
        /// </summary>
        public PayPalReportingService(PayPalReportingContext context)
            : base( context )
        {
        }

        /// <summary>
        /// Determines whether this instance can delete the specified item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="errorMessage">The error message.</param>
        /// <returns></returns>
        public virtual bool CanDelete(T item, out string errorMessage)
        {
            errorMessage = string.Empty;
            return true;
        }
    }
}
