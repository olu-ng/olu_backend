using System;

namespace OluBackendApp.DTOs
{
    public record ChatMetricsDto(
        int ChatId,
        int TotalMessages,
        double AverageResponseSeconds,
        DateTime Date
    );
}
