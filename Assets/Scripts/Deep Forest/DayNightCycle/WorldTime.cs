using System;
using System.Collections;
using UnityEngine;

namespace WorldTime
{ 
    public class WorldTime : MonoBehaviour
    {

        public event EventHandler<TimeSpan> WorldTimeChanged;

        [SerializeField]
        private float _dayLength; //length of in game day
        private TimeSpan _currentTime; // current imn game time
        private float _minuteLength => _dayLength/WorldTimeConstants.MinutesInDay;

        private void Start()
        {
            StartCoroutine(AddMinute());
        }


        private IEnumerator AddMinute()
        {
            _currentTime += TimeSpan.FromMinutes(1);
            WorldTimeChanged?.Invoke(this, _currentTime);
            yield return new WaitForSeconds(_minuteLength);
            StartCoroutine(AddMinute());
        }
    }
}