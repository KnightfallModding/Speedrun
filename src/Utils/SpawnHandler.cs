
namespace Speedrun;

public static class SpawnHandler
{
    public static int chosenSpawnPoint = 0;
    private static int _chosenSpawnPoint_Unmapped;
    public static int chosenSpawnPoint_Unmapped
    {
        get => _chosenSpawnPoint_Unmapped;
        set
        {
            _chosenSpawnPoint_Unmapped = value;
            shownSpawnPoint = _chosenSpawnPoint_Unmapped + 1;
        }
    }

    // Always chosenSpawnPoint_Unmapped+1
    public static int shownSpawnPoint = 1;
}