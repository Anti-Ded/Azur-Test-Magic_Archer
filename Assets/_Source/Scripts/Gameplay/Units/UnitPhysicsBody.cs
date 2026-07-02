using UnityEngine;

namespace MagicArcher.Gameplay.Units
{
    public static class UnitPhysicsBody
    {
        public static void Ensure(GameObject gameObject)
        {
            if (gameObject == null)
                return;

            if (gameObject.GetComponent<Rigidbody>() == null)
            {
                var body = gameObject.AddComponent<Rigidbody>();
                body.isKinematic = true;
                body.useGravity = false;
            }

            if (gameObject.GetComponent<Collider>() == null)
            {
                var capsule = gameObject.AddComponent<CapsuleCollider>();
                capsule.center = new Vector3(0f, 1f, 0f);
                capsule.height = 2f;
                capsule.radius = 0.45f;
            }
        }
    }
}
