using UnityEngine;
using ESS.Objects.Enemy;

namespace ESS.Objects.Player
{
    public class PlayerAttack : MonoBehaviour
    {
        public IEnemy target { get; protected set; }
    }
}