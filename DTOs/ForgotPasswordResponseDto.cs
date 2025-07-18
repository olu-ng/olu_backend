namespace OluBackendApp.DTOs
{
    // Dtos/ForgotPasswordResponseDto.cs
    public class ForgotPasswordResponseDto
    {
        /// <summary>
        /// Always true in the response, to avoid leaking whether the email exists.
        /// </summary>
        public bool RequiresOtp { get; set; }

        /// <summary>
        /// A generic message so that attackers can’t probe for valid emails.
        /// </summary>
        public string Message { get; set; } = default!;

        /// <summary>
        /// Client should call this next to verify the OTP.
        /// </summary>
        public string NextStep { get; set; } = "verify-forgot";
    }

}
