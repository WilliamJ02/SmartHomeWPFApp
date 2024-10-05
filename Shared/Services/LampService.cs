using Shared.Models;

namespace Shared.Services;

public class LampService : ILampService
{
    public Lamp Lamp { get; private set; }

    public LampService()
    {
        Lamp = new Lamp();
    }

    public void ToggleLamp()
    {
        Lamp.Toggled = !Lamp.Toggled;
    }
}
