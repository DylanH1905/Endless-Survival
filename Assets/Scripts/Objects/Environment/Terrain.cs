using ESS.Objects;

namespace ESS.Objects.Environment
{
    public class Terrain : EnvironmentCharacter
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
}