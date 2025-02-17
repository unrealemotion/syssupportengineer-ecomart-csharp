using System;
namespace JOIEnergy.Services
{
    public interface IDateTimeProvider
    {
        DateTime Now { get; }
    }
}