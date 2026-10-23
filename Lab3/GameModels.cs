using System;
using System.Windows.Media.Imaging;

public class EnemyTemplate
{
    public string EnemyName { get; set; }
    public string IconName { get; set; }
    public string BaseLife { get; set; }
    public string BaseGold { get; set; }
    public double LifeModifier { get; set; }
    public double GoldModifier { get; set; }
    public int SpawnChance { get; set; }
}

public class ActiveEnemy
{
    public string Name { get; private set; }

    // ИСПРАВЛЕНО: Изменили тип на BitmapImage, чтобы WPF мгновенно отображал картинку
    public BitmapImage IconPath { get; private set; }

    public BigNumber CurrentLife { get; private set; }
    public BigNumber RewardGold { get; private set; }

    public bool IsDead => CurrentLife.IsZero;

    public ActiveEnemy(EnemyTemplate template, int playerLevel)
    {
        Name = template.EnemyName;

        // Добавляем "Icons" в построение пути
        string iconsDirectory = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Icons");
        string fullPath = System.IO.Path.Combine(iconsDirectory, template.IconName ?? "default.jpg");


        if (System.IO.File.Exists(fullPath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(fullPath, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad; // Освобождает файл на диске, чтобы не было зависаний
                bitmap.EndInit();
                IconPath = bitmap;
            }
            catch
            {
                // Если картинка повреждена, останется пустой, но игра не вылетит
            }
        }

        // Формула: База + (База * Модификатор * Уровень)
        var baseLifeNum = new BigNumber(template.BaseLife);
        int lifeBonusMultiplier = (int)(template.LifeModifier * playerLevel);
        var lifeBonus = baseLifeNum.Multiply(lifeBonusMultiplier);
        CurrentLife = baseLifeNum.Add(lifeBonus);

        var baseGoldNum = new BigNumber(template.BaseGold);
        int goldBonusMultiplier = (int)(template.GoldModifier * playerLevel);
        var goldBonus = baseGoldNum.Multiply(goldBonusMultiplier);
        RewardGold = baseGoldNum.Add(goldBonus);
    }

    public void TakeDamage(BigNumber damage)
    {
        CurrentLife = CurrentLife.Subtract(damage);
    }
}

public class Player
{
    public int Level { get; private set; } = 1;
    public BigNumber Gold { get; private set; } = new BigNumber("0");
    public BigNumber Damage { get; private set; } = new BigNumber("50");

    public void AddGold(BigNumber amount)
    {
        Gold = Gold.Add(amount);
    }

    public bool TryUpgradeDamage(BigNumber cost)
    {
        if (Gold.CompareTo(cost) >= 0)
        {
            Gold = Gold.Subtract(cost);
            Damage = Damage.Add(new BigNumber("1"));
            Level++;
            return true;
        }
        return false;
    }
}
