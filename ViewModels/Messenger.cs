using System;
using System.Collections.Generic;

namespace BetaManager.ViewModels
{
    public static class Messenger
    {
        private static Dictionary<Type, Action<object>> _subscribers = new Dictionary<Type, Action<object>>();

        public static void Subscribe<TMessage> ( Action<TMessage> action )
        {
            Type messageType = typeof( TMessage );

            if ( !_subscribers.ContainsKey( messageType ) )
            {
                _subscribers[messageType] = message => action( ( TMessage )message );
            }
            else
            {
                _subscribers[messageType] += message => action( ( TMessage )message );
            }
        }

        public static void Send<TMessage> ( TMessage message )
        {
            Type messageType = typeof( TMessage );

            if ( _subscribers.ContainsKey( messageType ) )
            {
                _subscribers[messageType]?.Invoke( message );
            }
        }
    }
}