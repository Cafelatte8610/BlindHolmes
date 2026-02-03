using VContainer;
using VContainer.Unity;
using UnityEngine;

namespace BlindHolmes.MVP
{
    public class HolmesLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private HolmesView m_holmesView;
        [SerializeField]
        private EvidenceView m_evidenceView;
        [SerializeField]
        private GameManager m_GameManager;
        
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<HolmesPresenter>(Lifetime.Singleton)
                .AsImplementedInterfaces()
                .AsSelf();
            builder.RegisterComponent(m_holmesView);
            builder.RegisterComponent(m_evidenceView);
            builder.RegisterComponent(m_GameManager);
            builder.Register<HolmesModel>(Lifetime.Singleton);
        }
    }
}