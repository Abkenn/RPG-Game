using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class LivingCreature : INotifyPropertyChanged
    {
        private int currentHitPoints;

        public int CurrentHitPoints
        {
            get { return currentHitPoints; }
            set
            {
                currentHitPoints = value;
                OnPropertyChanged("CurrentHitPoints"); // при всяко set-ване (достъп за промяна на стойността) на променливата ще бъде стартирано събитието (event-a) PropertyChanged и ще се вика долната функция, която ще провери дали се е случило събитието и ще изпълни долния код
            }
        }

        public int MaximumHitPoints { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;


        public LivingCreature(int currentHP, int maximumHP)
        {
            CurrentHitPoints = currentHP;
            MaximumHitPoints = maximumHP;
        }

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); // PropertyChanged? проверява дали PropertyChanged != null, ако е различно, значи нещо се е Subscribe към събитието следователно изпращаме Notification към съответния клас
        }

    }
}
