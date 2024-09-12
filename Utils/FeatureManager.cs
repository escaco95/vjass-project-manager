using System.Diagnostics;
using System.Reflection;

namespace vJassMainJBlueprint.Utils
{
    /// <summary>
    /// 이 클래스는 구독-발행(pub-sub) 패턴을 통해 특정 클래스의 생성 시 관련 기능(feature)을
    /// 자동으로 초기화하는 역할을 합니다. 각 기능은 독립적으로 동작하며, 
    /// 클래스의 생성자를 수정하지 않고 기능을 추가하거나 제거할 수 있습니다.
    /// </summary>
    internal static class FeatureManager
    {
        /// <summary>
        /// 이 속성이 붙은 클래스는 특정 클래스의 생성 시 기능 초기화를 위해 자동으로 구독됩니다.
        /// </summary>
        [AttributeUsage(AttributeTargets.Class, Inherited = false)]
        internal class FeatureSubscriberAttribute : Attribute { }

        /// <summary>
        /// 정적 생성자. 어셈블리 내에서 [FeatureSubscriber] Attribute가 붙은 모든 클래스를 찾아 인스턴스를 생성합니다.
        /// 클래스가 처음으로 액세스될 때 한 번만 호출됩니다.
        /// </summary>
        static FeatureManager()
        {
            // 현재 어셈블리에서 [FeatureSubscriber] Attribute가 붙은 모든 클래스를 검색
            var typesWithSubscriberAttribute = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.GetCustomAttributes(typeof(FeatureSubscriberAttribute), true).Length != 0);

            // 검색된 각 클래스의 인스턴스를 생성
            foreach (var type in typesWithSubscriberAttribute)
            {
                Debug.WriteLine($"{DateTime.Now} [INFO] {type.FullName} 인스턴스 생성 중...");

                // 비공개 생성자도 호출할 수 있도록 인스턴스 생성
                Activator.CreateInstance(type, true);

                Debug.WriteLine($"{DateTime.Now} [INFO] {type.FullName} 인스턴스 생성 완료.");
            }
        }

        // 구독자 리스트를 관리하는 딕셔너리. 특정 타입의 메시지에 대한 구독자들을 저장합니다.
        private static readonly Dictionary<Type, List<Action<object>>> _subscriptions = [];

        /// <summary>
        /// 특정 타입의 메시지를 구독하는 메소드.
        /// 구독자는 이 메소드를 통해 해당 클래스에 대한 알림을 받을 수 있습니다.
        /// </summary>
        /// <typeparam name="T">구독할 클래스의 타입</typeparam>
        /// <param name="action">클래스가 생성되었을 때 처리할 구독자 함수</param>
        public static void SubscribeToFeature<T>(Action<T> action)
        {
            var subscriptionType = typeof(T);

            // 해당 타입에 대한 구독 리스트가 없으면 새로 생성
            if (!_subscriptions.TryGetValue(subscriptionType, out var subscribers))
            {
                subscribers = [];
                _subscriptions[subscriptionType] = subscribers;
            }

            // 구독자 리스트에 구독자 액션 추가
            subscribers.Add(msg => action((T)msg));
        }

        /// <summary>
        /// 클래스 생성 시 호출되며, 등록된 모든 구독자에게 알림을 전달하여 관련 기능을 초기화합니다.
        /// </summary>
        /// <typeparam name="T">생성된 클래스의 타입</typeparam>
        /// <param name="sender">생성된 클래스 인스턴스</param>
        public static void InitializeFeatures<T>(this T sender) where T : class
        {
            var subscriptionType = typeof(T);

            // 해당 타입에 대한 구독자가 존재하면, 각 구독자에게 메시지 전달
            if (_subscriptions.TryGetValue(subscriptionType, out var subscribers))
            {
                foreach (var subscriber in subscribers)
                {
                    subscriber(sender);
                }
            }
        }
    }
}
