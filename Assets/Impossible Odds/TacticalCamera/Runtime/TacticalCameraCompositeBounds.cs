using System;
using System.Collections.Generic;
using UnityEngine;

namespace ImpossibleOdds.TacticalCamera
{
    public class TacticalCameraCompositeBounds : AbstractTacticalCameraBounds
    {
        [SerializeField, Tooltip("Set of initial camera bounds.")]
        internal List<AbstractTacticalCameraBounds> initialCameraBounds;

        private List<ITacticalCameraBounds> cameraBounds;

        public IReadOnlyList<ITacticalCameraBounds> CameraBounds => cameraBounds;

        /// <summary>
        /// Add a new camera bounds to this composite bounds.
        /// Note: self-insertion is not allowed.
        /// </summary>
        /// <param name="newCameraBounds">The new camera bounds to add to this composite bounds.</param>
        public void Add(ITacticalCameraBounds newCameraBounds)
        {
            newCameraBounds.ThrowIfNull(nameof(newCameraBounds));

            if (ReferenceEquals(newCameraBounds, this))
            {
                throw new ArgumentException("Self-insertion is not allowed.");
            }

            if (!cameraBounds.Contains(newCameraBounds))
            {
                cameraBounds.Add(newCameraBounds);
            }
        }

        /// <summary>
        /// Removes the old camera bounds from this composite bounds.
        /// </summary>
        /// <param name="oldCameraBounds">The old camera bounds to be removed.</param>
        /// <returns>True if it was successfully removed.</returns>
        public bool Remove(ITacticalCameraBounds oldCameraBounds)
        {
            oldCameraBounds.ThrowIfNull(nameof(oldCameraBounds));
            return cameraBounds.Remove(oldCameraBounds);
        }

        /// <inheritdoc />
        public override void Apply(TacticalCamera tCamera)
        {
            tCamera.ThrowIfNull(nameof(tCamera));
            tCamera.transform.position = Apply(tCamera.transform.position);
        }

        /// <inheritdoc />
        public override Vector3 Apply(Vector3 position)
        {
            if (cameraBounds.IsNullOrEmpty() || IsWithinBounds(position))
            {
                return position;
            }

            ITacticalCameraBounds bestCandidate = null;
            Vector3 closestPosition = Vector3.zero;
            
            foreach (ITacticalCameraBounds cb in cameraBounds)
            {
                if (cb == null)
                {
                    continue;
                }

                if (bestCandidate == null)
                {
                    bestCandidate = cb;
                    closestPosition = bestCandidate.Apply(position);
                }
                else
                {
                    Vector3 potentialPosition = cb.Apply(position);
                    if (Vector3.Distance(position, closestPosition) > Vector3.Distance(position, potentialPosition))
                    {
                        bestCandidate = cb;
                        closestPosition = potentialPosition;
                    }
                }
            }

            return (bestCandidate != null) ? closestPosition : position;
        }

        /// <inheritdoc />
        public override bool IsWithinBounds(Vector3 position)
        {
            if (cameraBounds.IsNullOrEmpty())
            {
                return false;
            }

            foreach (ITacticalCameraBounds cb in cameraBounds)
            {
                if (cb == null)
                {
                    continue;
                }
                
                if (cb.IsWithinBounds(position))
                {
                    return true;
                }
            }

            return false;
        }

        private void Awake()
        {
            cameraBounds = new List<ITacticalCameraBounds>(initialCameraBounds);
        }
    }
}

