using UnityEngine;

namespace TetrisJenga.Pieces
{
    /// <summary>
    /// Handles collision detection for individual pieces
    /// </summary>
    public class PieceCollisionHandler : MonoBehaviour
    {
        private PieceController controller;
        private bool hasLanded = false;

        public void Initialize(PieceController pieceController)
        {
            controller = pieceController;
            hasLanded = false;
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (hasLanded || controller == null) return;

            // Check if we hit the base or another piece
            if (collision.gameObject.tag == Core.Constants.TAG_BASE ||
                collision.gameObject.tag == Core.Constants.TAG_PIECE ||
                collision.gameObject.layer == Core.Constants.LAYER_PIECE ||
                collision.gameObject.layer == LayerMask.NameToLayer("Default"))
            {
                // Check if we're hitting from above (landing)
                foreach (ContactPoint contact in collision.contacts)
                {
                    if (contact.normal.y > 0.5f) // Normal pointing up means we hit from above
                    {
                        hasLanded = true;
                        controller.OnPieceLanded(gameObject);
                        break;
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (hasLanded || controller == null) return;

            // Additional check for pieces that might already be in contact
            if (collision.gameObject.tag == Core.Constants.TAG_BASE ||
                collision.gameObject.tag == Core.Constants.TAG_PIECE)
            {
                Rigidbody rb = GetComponent<Rigidbody>();
                if (rb != null && rb.linearVelocity.magnitude < 0.1f)
                {
                    hasLanded = true;
                    controller.OnPieceLanded(gameObject);
                }
            }
        }
    }
}