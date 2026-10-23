using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows.Input;

namespace Lab3
{
    public class GameViewModel : ViewModelBase
    {
        private List<EnemyTemplate> _templates;
        private EnemyTemplate _currentTemplate; // [ИСПРАВЛЕНО]: Добавили поле для запоминания шаблона
        private ActiveEnemy _currentEnemy;
        private Player _player;
        private Random _random = new Random();

        public ActiveEnemy CurrentEnemy
        {
            get => _currentEnemy;
            private set { _currentEnemy = value; OnPropertyChanged(); }
        }

        public Player Player
        {
            get => _player;
            private set { _player = value; OnPropertyChanged(); }
        }

        public BigNumber UpgradeCost => Player.Damage.Multiply(10);

        public ICommand HitEnemyCommand { get; }
        public ICommand UpgradeDamageCommand { get; }
        public ICommand RepeatEnemyCommand { get; } // [ИСПРАВЛЕНО]: Добавили команду для кнопки Repeat
        public ICommand NextEnemyCommand { get; }   // [ИСПРАВЛЕНО]: Добавили команду для кнопки Next

        public GameViewModel()
        {
            _player = new Player();
            _templates = new List<EnemyTemplate>();

            LoadTemplates();
            SelectRandomEnemy();

            HitEnemyCommand = new RelayCommand(ExecuteHitEnemy);
            UpgradeDamageCommand = new RelayCommand(ExecuteUpgradeDamage);

            // [ИСПРАВЛЕНО]: Связали новые кнопки с методами логики
            RepeatEnemyCommand = new RelayCommand(ExecuteRepeatEnemy);
            NextEnemyCommand = new RelayCommand(ExecuteNextEnemy);
        }

        private void LoadTemplates()
        {
            string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "enemies.json");

            if (File.Exists(filePath))
            {
                try
                {
                    string[] lines = File.ReadAllLines(filePath);
                    foreach (string line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string[] parts = line.Split(';');
                        if (parts.Length >= 6)
                        {
                            var enemy = new EnemyTemplate();
                            enemy.EnemyName = parts[0];
                            enemy.BaseLife = parts[2];
                            enemy.BaseGold = parts[3];

                            double.TryParse(parts[4].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double lifeMod);
                            double.TryParse(parts[5].Replace(',', '.'), NumberStyles.Any, CultureInfo.InvariantCulture, out double goldMod);

                            enemy.LifeModifier = lifeMod;
                            enemy.GoldModifier = goldMod;

                            if (parts.Length >= 7 && int.TryParse(parts[6], out int chance))
                                enemy.SpawnChance = chance;
                            else
                                enemy.SpawnChance = 100;

                            if (enemy.EnemyName.Contains("Enki")) enemy.IconName = "Enki.jpg";
                            else if (enemy.EnemyName.Contains("D'arce")) enemy.IconName = "D'arce.jpg";
                            else if (enemy.EnemyName.Contains("Cahara")) enemy.IconName = "Cahara.jpg";
                            else if (enemy.EnemyName.Contains("Ragnvalder")) enemy.IconName = "Ragnvalder.jpg";
                            else enemy.IconName = "default.png";

                            _templates.Add(enemy);
                        }
                    }
                }
                catch
                {
                    // Игнорируем ошибку
                }
            }

            if (_templates.Count == 0)
            {
                _templates.Add(new EnemyTemplate { EnemyName = "Enki", BaseLife = "60", BaseGold = "10", LifeModifier = 1.5, GoldModifier = 1.2, SpawnChance = 70, IconName = "Enki.jpg" });
                _templates.Add(new EnemyTemplate { EnemyName = "D'arce", BaseLife = "80", BaseGold = "5", LifeModifier = 1.4, GoldModifier = 1.0, SpawnChance = 60, IconName = "D'arce.jpg" });
                _templates.Add(new EnemyTemplate { EnemyName = "Cahara", BaseLife = "70", BaseGold = "25", LifeModifier = 1.2, GoldModifier = 2.5, SpawnChance = 55, IconName = "Cahara.jpg" });
                _templates.Add(new EnemyTemplate { EnemyName = "Ragnvalder", BaseLife = "90", BaseGold = "2", LifeModifier = 2.5, GoldModifier = 0.5, SpawnChance = 40, IconName = "Ragnvalder.jpg" });
            }
        }

        private void SelectRandomEnemy()
        {
            if (_templates == null || _templates.Count == 0) return;

            int totalWeight = _templates.Sum(t => t.SpawnChance);
            int roll = _random.Next(0, totalWeight);
            int currentWeight = 0;

            EnemyTemplate selected = _templates.First();
            foreach (var temp in _templates)
            {
                currentWeight += temp.SpawnChance;
                if (roll < currentWeight)
                {
                    selected = temp;
                    break;
                }
            }

            _currentTemplate = selected; // [ИСПРАВЛЕНО]: Вот то самое Куда добавить №2 (сохраняем выбранный шаблон)
            CurrentEnemy = new ActiveEnemy(selected, Player.Level);
        }

        private void ExecuteHitEnemy()
        {
            if (CurrentEnemy == null) return;

            CurrentEnemy.TakeDamage(Player.Damage);

            if (CurrentEnemy.IsDead)
            {
                Player.AddGold(CurrentEnemy.RewardGold);
                SelectRandomEnemy();
            }

            OnPropertyChanged(nameof(CurrentEnemy));
            OnPropertyChanged(nameof(Player));
            OnPropertyChanged(nameof(UpgradeCost));
        }

        private void ExecuteUpgradeDamage()
        {
            if (Player.TryUpgradeDamage(UpgradeCost))
            {
                OnPropertyChanged(nameof(Player));
                OnPropertyChanged(nameof(UpgradeCost));
            }
        }

        // [ИСПРАВЛЕНО]: Метод для кнопки Repeat (перерождает того же самого врага)
        private void ExecuteRepeatEnemy()
        {
            if (_currentTemplate != null)
            {
                CurrentEnemy = new ActiveEnemy(_currentTemplate, Player.Level);
                OnPropertyChanged(nameof(CurrentEnemy));
            }
        }

        // [ИСПРАВЛЕНО]: Метод для кнопки Next (просто выбирает нового случайного врага)
        private void ExecuteNextEnemy()
        {
            SelectRandomEnemy();
        }
    }
}
