using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionMachine
{
    /// <summary>
    /// The gestures available to be used in the transitions.
    /// </summary>
    public enum Gesture
    {
        None,
        Wave,
    }

    /// <summary>
    /// The TransitionBuilder class is used to set all the parameters that will be used by a Transition.
    /// </summary>
    public class TransitionBuilder
    {
        /// <summary>
        /// Gets the gesture that will act as a trigger for the next state.
        /// </summary>
        public Gesture Gesture { get; private set; }

        /// <summary>
        /// Gets the point of interest that will be used by some triggers.
        /// </summary>
        public Vector3 PointOfInterest { get; private set; }

        /// <summary>
        /// Gets the distance from the point of interest that will be accepted to claim that the user is looking at it.
        /// </summary>
        public double PointOfInterestRadius { get; private set; }

        /// <summary>
        /// Gets the minimum distance from the point of interest that will be required for the user to be.
        /// </summary>
        public double MinDistance { get; private set; }

        /// <summary>
        /// Gets the maximum distance from the point of interest that will be accepted for the user to be.
        /// </summary>
        public double MaxDistance { get; private set; }

        /// <summary>
        /// True if the user entering the zone between MinDistance and the MaxDistance from the point of interest will trigger the next state.
        /// </summary>
        public bool IsTriggerZone { get; private set; }

        /// <summary>
        /// Gets the minimum time that the user will have to avoid moving.
        /// </summary>
        public double StaticTime { get; private set; }

        /// <summary>
        /// Gets the minimum time that the user will have to look at the point of interest.
        /// </summary>
        public double FocusTime { get; private set; }

        /// <summary>
        /// Gets the minimum time that the user will have to look away from the point of interest.
        /// </summary>
        public double NonFocusTime { get; private set; }

        private bool _isReady;

        /// <summary>
        /// The constructor.
        /// </summary>
        public TransitionBuilder()
        {
            MinDistance = 0;
            MaxDistance = -1;
            IsTriggerZone = false;
            StaticTime = 0;
            FocusTime = 0;
            NonFocusTime = -1;
            PointOfInterestRadius = 1.5;
            Gesture = Gesture.None;
        }

        /// <summary>
        /// Sets the gesture that will be required for the user to do.
        /// </summary>
        /// <param name="gesture">The gesture to be set.</param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="InteractionNDFA.Gesture" /> for all gestures available.
        public TransitionBuilder SetGesture(Gesture gesture)
        {
            this.Gesture = gesture;
            return this;
        }

        /// <summary>
        /// Sets the point of interest that will be used by other conditions.
        /// </summary>
        /// <param name="coords">The coordinates of the point of interest.</param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetMinimumDistance(double)"/>
        /// <see cref="TransitionBuilder.SetMinimumDistance(double)"/>
        /// <see cref="TransitionBuilder.SetFocusTime(double)"/>
        /// <see cref="TransitionBuilder.SetNonFocusTime(double)"/>
        public TransitionBuilder SetPointOfInterest(Vector3 coords)
        {
            PointOfInterest = coords;
            return this;
        }

        /// <summary>
        /// Sets the minimum acceptable distance from the user to the point of interest.
        /// By default the minimum distance is set to zero.
        /// </summary>
        /// <param name="distance">The minimum distance to be set.</param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetPointOfInterest(Vector3)"/>
        public TransitionBuilder SetMinimumDistance(double distance)
        {
            MinDistance = distance;
            return this;
        }

        /// <summary>
        /// Sets the maximum acceptable distance from the user to the point of interest. 
        /// By default the maximum distance is set to infinity.
        /// </summary>
        /// <param name="distance">The minimum distance to be set. </param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetPointOfInterest(Vector3)"/>
        public TransitionBuilder SetMaximumDistance(double distance)
        {
            MaxDistance = distance;
            return this;
        }

        /// <summary>
        /// Sets the zone between the minimum acceptable distance and maximum acceptable distance
        /// to be the trigger of the next state.
        /// </summary>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetMinimumDistance(double)"/>
        /// <see cref="TransitionBuilder.SetMaximumDistance(double)"/>
        public TransitionBuilder SetAsTriggerZone()
        {
            IsTriggerZone = true;
            return this;
        }

        /// <summary>
        /// Sets the minimum time that the user will have to avoid moving.
        /// </summary>
        /// <param name="seconds">The minimum time to be set in seconds. </param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetPointOfInterest(Vector3)"/>
        public TransitionBuilder SetStaticTime(double seconds)
        {
            StaticTime = seconds;
            return this;
        }

        /// <summary>
        /// Sets the minimum time that the user will have to look at (or near) the point of interest.
        /// </summary>
        /// <param name="seconds">The minimum time to be set in seconds. </param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetPointOfInterest(Vector3)"/>
        public TransitionBuilder SetFocusTime(double seconds)
        {
            FocusTime = seconds;
            return this;
        }

        /// <summary>
        /// Sets the minimum time that the user will have to look away from the point of interest.
        /// </summary>
        /// <param name="seconds">The minimum time to be set in seconds. </param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetPointOfInterest(Vector3)"/>
        public TransitionBuilder SetNonFocusTime(double seconds)
        {
            NonFocusTime = seconds;
            return this;
        }

        /// <summary>
        /// Sets the radius at which looking at the point of interest will be accepted.
        /// </summary>
        /// <param name="radius">The radius to be set.</param>
        /// <returns>This transition builder.</returns>
        /// See <see cref="TransitionBuilder.SetFocusTime(double)"/>
        /// <see cref="TransitionBuilder.SetNonFocusTime(double)"/>
        public TransitionBuilder SetRadius(double radius)
        {
            PointOfInterestRadius = radius;
            return this;
        }

        /// <summary>
        /// Creates the final Transition object.
        /// </summary>
        /// <returns>The transition.</returns>
        /// <exception cref="System.Exception">Thrown when no gesture or point of interest is set.</exception>
        /// <exception cref="System.Exception">Thrown when the minimum focus time is less than zero.</exception>
        /// <exception cref="System.Exception">Thrown when the minimum distance is less than zero.</exception>
        /// <exception cref="System.Exception">Thrown when the maximum distance is less than the minimum.</exception>
        /// <exception cref="System.Exception">Thrown when both distraction time and focus time are set at the same time.</exception>
        /// <exception cref="System.Exception">Thrown when the radius is less than zero.</exception>
        public Transition Build()
        {
            if (HasPoi() && StaticTime > 0)
            {
                throw new Exception("Static time can't be set along with a point of interest.");
            }
            if (!HasGesture() && !HasPoi() && StaticTime == 0)
            {
                throw new Exception("A gesture or point of interest is required if no static time is provided.");
            }
            if (StaticTime < 0)
            {
                throw new Exception("Static time can't be negative.");
            }
            if (FocusTime < 0)
            {
                throw new Exception("Focus time can't be negative.");
            }
            if (MinDistance < 0)
            {
                throw new Exception("Minimum distance can't be negative.");
            }
            if (FocusTime > 0 && NonFocusTime >= 0)
            {
                throw new Exception("Can't set both distraction time and focus time at the same time.");
            }
            if (MaxDistance >= 0 && MinDistance > MaxDistance)
            {
                throw new Exception("Can't set the mininum distance greater than the maximum distance.");
            }
            if (PointOfInterestRadius < 0)
            {
                throw new Exception("Radius can't be negative.");
            }

            if (!HasPoi() && FocusTime > 0)
            {
                Debug.LogWarning("Focus time is set but no point of interest was provided.");
            }
            if (!HasPoi() && NonFocusTime >= 0)
            {
                Debug.LogWarning("Distraction time is set but no point of interest was provided.");
            }
            _isReady = true;
            return new Transition(this);
        }

        private bool HasGesture()
        {
            return Gesture != Gesture.None;
        }

        private bool HasPoi()
        {
            return PointOfInterest != default;
        }

        public bool IsReady()
        {
            return _isReady;
        }
    }

    /// <summary>
    /// The Transition class groups conditions thats needs to be met in order to change from 
    /// the current state to another. Examples of transitions are:
    /// <list type="bullet">
    /// <item>
    /// <description>Getting to far from a point.</description>
    /// </item>
    /// <item>
    /// <description>Looking at some point a certain amount of time.</description>
    /// </item>
    /// <item>
    /// <description>Not looking at some point a certain amount of time.</description>
    /// </item>
    /// <item>
    /// <description>Making a gesture at some distance of a point, after looking at it a certain amount of time.</description>
    /// </item>
    /// </list>
    /// </summary>
    public class Transition
    {
        private readonly Gesture _gesture;
        private Vector3 _pointOfInterest;
        private readonly double _pointOfInterestRadius;
        private readonly double _minDistance;
        private readonly double _maxDistance;
        private readonly bool _isTriggerZone;
        private readonly double _staticTime;
        private readonly double _focusTime;
        private readonly double _nonFocusTime;

        private double _currentStaticTime;
        private double _currentFocusTime;
        private double _currentNonFocusTime;

        private Vector3 _lastPosition;

        private bool _inRange;
        private bool _isStatic;
        private bool _isFocusing;
        private bool _isDistracted;

        /// <summary>
        /// The constructor of a transition. Do not create objects directly, use TransitionBuilder instead.
        /// </summary>
        /// <param name="builder">The transition builder with all settings.</param>
        /// See <see cref="TransitionBuilder"/>
        public Transition(TransitionBuilder builder)
        {
            if (!builder.IsReady())
            {
                throw new Exception("Builder is not ready.");
            }
            _minDistance = builder.MinDistance;
            _maxDistance = builder.MaxDistance;
            _isTriggerZone = builder.IsTriggerZone;
            _staticTime = builder.StaticTime;
            _focusTime = builder.FocusTime;
            _nonFocusTime = builder.NonFocusTime;
            _pointOfInterest = builder.PointOfInterest;
            _pointOfInterestRadius = builder.PointOfInterestRadius;
            _gesture = builder.Gesture;
            _currentStaticTime = 0;
            _currentFocusTime = 0;
            _currentNonFocusTime = 0;
            _inRange = false;
            _isStatic = false;
            _isFocusing = false;
            _isDistracted = false;
        }

        /// <summary>
        /// Resets the transition trackers.
        /// </summary>
        public void Reset()
        {
            _currentFocusTime = 0;
            _currentNonFocusTime = 0;
            _inRange = false;
            _isStatic = false;
            _isFocusing = false;
            _isDistracted = false;
        }

        /// <summary>
        /// Notifies this transition that the user is looking somewhere.
        /// </summary>
        /// <param name="origin">The position of the user.</param>
        /// <param name="direction">The direction at which the user is looking at.</param>
        /// <param name="elapsed">The amount of time the user spent looking there.</param>
        /// <returns>True if the transition meets all the conditions. False otherwise.</returns>
        public bool Notify(Vector3 origin, Vector3 direction, double elapsed)
        {
            if (HasPoi())
            {
                VerifyPosition(origin);
                if (_isTriggerZone)
                    return _inRange;
                else
                {
                    if (UserIsFocusing(origin, direction))
                    {
                        AddFocusTime(elapsed);
                        if (!HasGesture() && _isFocusing && _inRange)
                            return true;
                    }
                    else
                    {
                        AddNonFocusTime(elapsed);
                        if (!HasGesture() && _isDistracted && _inRange)
                            return true;
                    }

                }
            }
            else
            {
                if (UserIsStatic(origin))
                {
                    AddStaticTime(elapsed);
                    if (_isStatic)
                        return true;
                }
            }
            return false;
        }


        /// <summary>
        /// Notifies this transition that the user has made a gesture.
        /// </summary>
        /// <param name="gesture">The gesture done by the user.</param>
        /// <param name="origin">The position of the user.</param>
        /// <param name="direction">The direction at which the user is looking at.</param>
        /// <returns>True if the transition meets all the conditions. False otherwise.</returns>
        public bool Notify(Gesture gesture, Vector3 origin, Vector3 direction)
        {
            if (!HasGesture())
                return Notify(origin, direction, 0);

            if (HasPoi() && UserIsFocusing(origin, direction))
                return _focusTime > 0 ? gesture == _gesture && _isFocusing && _inRange : gesture == _gesture && _inRange;

            else if (HasPoi() && !UserIsFocusing(origin, direction) && _nonFocusTime >= 0)
                return gesture == _gesture && _isDistracted && _inRange;

            else if (!HasPoi())
                return gesture == _gesture;

            return false;
        }

        private void AddStaticTime(double seconds)
        {
            _currentStaticTime += seconds;
            if (_currentStaticTime >= _staticTime)
                _isStatic = true;
        }

        private void AddFocusTime(double seconds)
        {
            _currentNonFocusTime = 0;
            _currentFocusTime += seconds;
            if (_currentFocusTime >= _focusTime)
            {
                _isFocusing = _focusTime > 0;
                _isDistracted = false;
            }
        }

        private void AddNonFocusTime(double seconds)
        {
            _currentFocusTime = 0;
            _currentNonFocusTime += seconds;
            if (_currentNonFocusTime >= _nonFocusTime)
            {
                _isFocusing = false;
                _isDistracted = _nonFocusTime >= 0;
            }
        }

        private void VerifyPosition(Vector3 coords)
        {
            var distance = Vector3.Distance(coords, _pointOfInterest);
            _inRange = distance >= _minDistance && (_maxDistance < 0 || distance <= _maxDistance);
        }

        private bool UserIsFocusing(Vector3 origin, Vector3 direction)
        {
            var distance = Vector3.Distance(origin, _pointOfInterest);
            var translatedOrigin = origin + distance * direction;
            return Vector3.Distance(translatedOrigin, _pointOfInterest) < _pointOfInterestRadius;
        }

        private bool UserIsStatic(Vector3 origin)
        {
            if (_lastPosition != origin)
            {
                _lastPosition = origin;
                return false;
            }
            return true;
        }

        private bool HasGesture()
        {
            return _gesture != Gesture.None;
        }

        private bool HasPoi()
        {
            return _pointOfInterest != default;
        }
    }

    /// <summary>
    /// The Inreaction class is used to create finite state machines where transitions are given by the user actions.
    /// </summary>
    public class Interaction
    {
        class State
        {
            public Action Callback { get; set; }
            private readonly Dictionary<Transition, string> _transitions = new Dictionary<Transition, string>();

            public State(Action callback)
            {
                Callback = callback;
            }

            public void AddTransition(Transition transition, string target)
            {
                _transitions.Add(transition, target);
            }

            public string Notify(Gesture gesture, Vector3 origin, Vector3 direction)
            {
                foreach (Transition t in _transitions.Keys)
                {
                    if (t.Notify(gesture, origin, direction))
                        return _transitions[t];
                }
                return "";
            }

            public string Notify(Vector3 origin, Vector3 direction, double elapsed)
            {
                foreach (Transition t in _transitions.Keys)
                {
                    if (t.Notify(origin, direction, elapsed))
                        return _transitions[t];
                }
                return "";
            }

            public void Reset()
            {
                foreach (Transition t in _transitions.Keys)
                {
                    t.Reset();
                }
            }
        }

        private readonly Dictionary<string, State> _states;
        private string current;

        /// <summary>
        /// The constructor.
        /// </summary>
        public Interaction()
        {
            _states = new Dictionary<string, State>();
            current = "";
        }

        /// <summary>
        /// Adds a state to the machine.
        /// </summary>
        /// <param name="name">The name of the state.</param>
        /// <param name="callback">A method that will be executed when the state is reached.</param>
        /// <returns>This interaction.</returns>
        /// <exception cref="System.Exception">Thrown when the name is the empty string.</exception>
        /// <exception cref="System.Exception">Thrown when the name used.</exception>
        public Interaction AddState(string name, Action callback)
        {
            if (name.Length == 0)
            {
                throw new Exception($"Tried to add a state without a name.");
            }
            if (_states.ContainsKey(name))
            {
                throw new Exception($"Tried to add already existent state '{name}'.");
            }
            var state = new State(callback);
            _states.Add(name, state);
            return this;
        }

        /// <summary>
        /// Adds a transition between two states.
        /// </summary>
        /// <param name="from">The name of the state from which the transition will start.</param>
        /// <param name="to">The name of the state that the transition will lead to.</param>
        /// <param name="transition">The transition.</param>
        /// <returns>This interaction.</returns>
        /// <exception cref="System.Exception">Thrown when one of the states doesn't exists.</exception>
        /// <see cref="Transition" />
        public Interaction AddTransition(string from, string to, Transition transition)
        {
            if (!_states.ContainsKey(to))
                throw new Exception($"Tried to add transition to a nonexistent state '{to}'.");
            if (!_states.ContainsKey(from))
                throw new Exception($"Tried to add transition from a nonexistent state '{from}'.");

            _states[from].AddTransition(transition, to);
            return this;
        }

        /// <summary>
        /// Sets the inital state of the machine.
        /// </summary>
        /// <param name="name">The name of the inital state.</param>
        /// <returns>This interaction.</returns>
        public Interaction SetInitialState(string name)
        {
            current = name;
            return this;
        }

        /// <summary>
        /// Starts the machine.
        /// </summary>
        /// <returns>This interaction.</returns>
        /// <exception cref="System.Exception">Thrown when no inital state was set or doesn't exists.</exception>
        public Interaction Start()
        {
            if (current == "")
                throw new Exception($"Missing initial state.");
            if (!_states.ContainsKey(current))
                throw new Exception($"State not found: {current}");

            _states[current].Callback();
            return this;
        }

        /// <summary>
        /// Notifies the current state that the user has made a gesture.
        /// </summary>
        /// <param name="gesture">The gesture made by the user.</param>
        /// <param name="origin">The position of the user.</param>
        /// <param name="direction">The direction at which the user is looking at.</param>
        public void Notify(Gesture gesture, Vector3 origin, Vector3 direction)
        {
            var nextState = _states[current].Notify(gesture, origin, direction);
            ChangeState(nextState);
        }

        /// <summary>
        /// Notifies the current state that the user is looking somewhere.
        /// </summary>
        /// <param name="origin">The position of the user.</param>
        /// <param name="direction">The direction at which the user is looking at.</param>
        /// <param name="elapsed">The time spent looking at the direction.</param>
        public void Notify(Vector3 origin, Vector3 direction, double elapsed)
        {
            var nextState = _states[current].Notify(origin, direction, elapsed);
            ChangeState(nextState);
        }

        private void ChangeState(string newState)
        {
            if (newState == "")
                return;
            _states[current].Reset();
            current = newState;
            Start();
        }
    }
}