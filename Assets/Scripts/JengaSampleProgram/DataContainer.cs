public class DataContainer
{
    public static DataContainer Instance => _instance ??= new DataContainer();
    public int SelectedBlockId { get; set; } = -1;
    public float CollapseProbability { get; set; } = 1.0f;
    public bool IsGameFinish { get; set; } = false;

    private static DataContainer _instance = null;
}
