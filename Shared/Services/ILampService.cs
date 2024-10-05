using Shared.Models;

namespace Shared.Services;

public interface ILampService
{
    Lamp Lamp { get; }
    void ToggleLamp();
}
