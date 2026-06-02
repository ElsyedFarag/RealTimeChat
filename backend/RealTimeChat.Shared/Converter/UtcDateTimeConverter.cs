using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace RealTimeChat.Shared.Converter;

public class UtcDateTimeConverter : ValueConverter<DateTime, DateTime>
{
    public UtcDateTimeConverter()
        : base(
            v => v,
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc))
    {
    }
}
