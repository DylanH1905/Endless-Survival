using ESS.Objects;

class ITerrain : EnvironmentCharacter
{
    private void Awake()
    {
        InitializeTerrain();
    }
    private void InitializeTerrain()
    {
        objectType = EnvironmentObjectType.Terrain;
        blocksMovement = true;
        effectObject = false;
        effect = new ObjectEffect();
    }
}