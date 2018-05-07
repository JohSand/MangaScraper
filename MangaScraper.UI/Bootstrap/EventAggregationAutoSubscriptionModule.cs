using Autofac;
using Autofac.Core;
using Caliburn.Micro;

namespace MangaScraper.UI.Bootstrap {
  public class EventAggregationAutoSubscriptionModule : Module {
    protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration) {
      registration.Activated += OnComponentActivated;
    }

    static void OnComponentActivated(object sender, ActivatedEventArgs<object> e) { //  we never want to fail, so check for null (should never happen), and return if it is
      if (e == null)
        return;
      //  try to convert instance to IHandle
      //  I originally did e.Instance.GetType().IsAssignableTo<>() and then 'as', 
      //  but it seemed redundant
      dynamic handler = e.Instance;
      SubscribeOnHandler(e, handler);
    }

    private static void SubscribeOnHandler<T>(ActivatedEventArgs<object> e, object handler) {
      
    }

    private static void SubscribeOnHandler<T>(ActivatedEventArgs<object> e, IHandle<T> handler) {
      //  if it is not null, it implements, so subscribe
      e.Context.Resolve<IEventAggregator>().SubscribeOnPublishedThread(handler);
    }
  }
}