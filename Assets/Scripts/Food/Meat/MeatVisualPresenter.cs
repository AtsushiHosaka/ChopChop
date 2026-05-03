using UnityEngine;

namespace ChopChop.Food.Meat
{
    [AddComponentMenu("ChopChop/Food/Meat Visual Presenter")]
    public sealed class MeatVisualPresenter : MonoBehaviour
    {
        [SerializeField] private MeatTearController tearController;
        [SerializeField] private GameObject intactVisual = null;
        [SerializeField] private GameObject tornVisual = null;
        [SerializeField] private ParticleSystem tearParticles = null;
        [SerializeField] private Animator animator;
        [SerializeField] private string tornTrigger = "Torn";

        private void Reset()
        {
            tearController = GetComponentInParent<MeatTearController>();
            animator = GetComponent<Animator>();
        }

        private void OnEnable()
        {
            if (tearController != null)
            {
                tearController.Torn += HandleTorn;
            }

            SetTornVisual(false);
        }

        private void OnDisable()
        {
            if (tearController != null)
            {
                tearController.Torn -= HandleTorn;
            }
        }

        public void SetTornVisual(bool isTorn)
        {
            if (intactVisual != null)
            {
                intactVisual.SetActive(!isTorn);
            }

            if (tornVisual != null)
            {
                tornVisual.SetActive(isTorn);
            }
        }

        private void HandleTorn(MeatTearEvent tearEvent)
        {
            SetTornVisual(true);

            if (tearParticles != null)
            {
                tearParticles.transform.position = tearEvent.WorldPoint;
                tearParticles.Play();
            }

            if (animator != null && !string.IsNullOrWhiteSpace(tornTrigger))
            {
                animator.SetTrigger(tornTrigger);
            }
        }
    }
}
