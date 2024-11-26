namespace IdentityServiceApi.Interfaces.Utilities
{
    /// <summary>
    ///     Defines a contract for parameter validation methods.
    ///     This interface specifies methods for validating input parameters 
    ///     in services, ensuring that they meet required conditions before 
    ///     being processed.
    /// </summary>
    /// <remarks>
    ///     @Author: Christian Briglio
    ///     @Created: 2024
    /// </remarks>
    public interface IParameterValidator
    {
        /// <summary>
        ///     Validates that a string parameter is not null or empty.
        ///     Implementations should throw an exception if the parameter
        ///     does not meet this requirement.
        /// </summary>
        /// <param name="parameter">
        ///     The string parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        void ValidateNotNullOrEmpty(string parameter, string parameterName);


        /// <summary>
        ///     Validates that an object parameter is not null.
        ///     Implementations should throw an exception if the 
        ///     parameter is null.
        /// </summary>
        /// <param name="parameter">
        ///     The object parameter to validate.
        /// </param>
        /// <param name="parameterName">
        ///     The name of the parameter (used in the exception message).
        /// </param>
        void ValidateObjectNotNull(object parameter, string parameterName);
    }
}
