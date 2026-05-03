using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using ChopChop.Food.Meat;
using UnityEngine;

namespace ChopChop.Food.Tearing
{
    [AddComponentMenu("ChopChop/Food/Obi Tearable Cloth Reflection Driver")]
    public sealed class ObiTearableClothReflectionDriver : MeatTearDriverBehaviour
    {
        [Tooltip("Assign an ObiTearableCloth component after importing Obi Cloth.")]
        [SerializeField] private Component obiTearableCloth = null;

        private EventInfo tornEvent;
        private Delegate tornDelegate;
        private bool hasLoggedMissingObi;

        public override void Configure(MeatTearProfile profile)
        {
            if (obiTearableCloth == null || profile == null)
            {
                return;
            }

            SetMember("tearResistanceMultiplier", profile.tearResistanceMultiplier);
            SetMember("tearRate", profile.tearRate);
            SetMember("tearDebilitation", profile.tearDebilitation);
        }

        public override bool RequestTearAt(Vector3 worldPoint, MeatTearReason reason)
        {
            if (obiTearableCloth == null)
            {
                LogMissingObi();
                return false;
            }

            var tearAtPoint = obiTearableCloth.GetType().GetMethod(
                "TearAt",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                new[] { typeof(Vector3) },
                null);

            if (tearAtPoint == null)
            {
                return false;
            }

            tearAtPoint.Invoke(obiTearableCloth, new object[] { worldPoint });
            return true;
        }

        private void OnEnable()
        {
            TrySubscribe();
        }

        private void OnDisable()
        {
            TryUnsubscribe();
        }

        private void TrySubscribe()
        {
            if (obiTearableCloth == null)
            {
                return;
            }

            tornEvent = obiTearableCloth.GetType().GetEvent("OnClothTorn", BindingFlags.Instance | BindingFlags.Public);
            if (tornEvent == null)
            {
                LogMissingObi();
                return;
            }

            tornDelegate = CreateTornDelegate(tornEvent.EventHandlerType);
            if (tornDelegate == null)
            {
                Debug.LogWarning("[Meat Tear] Obi OnClothTorn signature was not compatible with the reflection driver.", this);
                return;
            }

            tornEvent.AddEventHandler(obiTearableCloth, tornDelegate);
        }

        private void TryUnsubscribe()
        {
            if (obiTearableCloth == null || tornEvent == null || tornDelegate == null)
            {
                return;
            }

            tornEvent.RemoveEventHandler(obiTearableCloth, tornDelegate);
            tornEvent = null;
            tornDelegate = null;
        }

        private Delegate CreateTornDelegate(Type eventHandlerType)
        {
            var invoke = eventHandlerType.GetMethod("Invoke");
            if (invoke == null)
            {
                return null;
            }

            var parameters = invoke.GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType, parameter.Name))
                .ToArray();
            var handler = GetType().GetMethod(nameof(HandleObiTorn), BindingFlags.Instance | BindingFlags.NonPublic);
            Expression body;

            if (parameters.Length == 0)
            {
                body = Expression.Call(Expression.Constant(this), handler, Expression.Constant(null, typeof(object)));
            }
            else
            {
                body = Expression.Call(
                    Expression.Constant(this),
                    handler,
                    Expression.Convert(parameters[parameters.Length - 1], typeof(object)));
            }

            return Expression.Lambda(eventHandlerType, body, parameters).Compile();
        }

        private void HandleObiTorn(object tearInfo)
        {
            var worldPoint = TryReadWorldPoint(tearInfo, out var point)
                ? point
                : transform.position;
            RaisePhysicalTorn(worldPoint);
        }

        private bool TryReadWorldPoint(object tearInfo, out Vector3 point)
        {
            point = Vector3.zero;
            if (tearInfo == null)
            {
                return false;
            }

            var type = tearInfo.GetType();
            var property = type.GetProperty("worldPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? type.GetProperty("WorldPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.PropertyType == typeof(Vector3))
            {
                point = (Vector3)property.GetValue(tearInfo);
                return true;
            }

            var field = type.GetField("worldPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                ?? type.GetField("WorldPosition", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null && field.FieldType == typeof(Vector3))
            {
                point = (Vector3)field.GetValue(tearInfo);
                return true;
            }

            return false;
        }

        private void SetMember(string memberName, object value)
        {
            var type = obiTearableCloth.GetType();
            var field = type.GetField(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(obiTearableCloth, value);
                return;
            }

            var property = type.GetProperty(memberName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (property != null && property.CanWrite)
            {
                property.SetValue(obiTearableCloth, value);
            }
        }

        private void LogMissingObi()
        {
            if (hasLoggedMissingObi)
            {
                return;
            }

            hasLoggedMissingObi = true;
            Debug.LogWarning("[Meat Tear] Assign an ObiTearableCloth component after importing Obi Cloth. Gameplay tears still work, but visual tearing needs an Obi adapter.", this);
        }
    }
}
