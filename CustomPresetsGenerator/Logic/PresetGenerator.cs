using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SpirePresetsGenerator.Models;
using SpirePresetsGenerator.Services;

namespace SpirePresetsGenerator.Logic
{
    public sealed class PresetGenerator
    {
        private static readonly Random _globalRnd = new Random();
        private readonly Random _rnd;

        public PresetGenerator()
        {
            // Используем глобальный Random для генерации уникального seed'а
            int seed;
            lock (_globalRnd)
            {
                seed = _globalRnd.Next();
            }
            _rnd = new Random(seed);
        }

        public List<PresetFile> GeneratePresets(int count, string type, string author, ArpConfig arp)
        {
            if (count < 1 || count > 1000) throw new ArgumentOutOfRangeException("count", "Укажи количество от 1 до 1000");
            var files = new List<PresetFile>(count);
            
            // Список всех доступных типов для режима random
            var allTypes = new[] { "lead", "pad", "bass", "pluck", "fx", "sequence", "atmo", "key", "gt", "sy" };
            
            for (int i = 0; i < count; i++)
            {
                // В режиме random выбираем случайный тип для каждого пресета
                string actualType = type;
                if (string.Equals(type, "random", StringComparison.OrdinalIgnoreCase))
                {
                    actualType = allTypes[RandomInt(0, allTypes.Length - 1)];
                }
                
                var preset = GeneratePreset(i, actualType, author, arp);
                string suffix = (arp != null && arp.Enabled) ? "_arp" : string.Empty;
                string name = actualType + "_preset" + suffix + "_" + (i + 1).ToString("D3", CultureInfo.InvariantCulture) + ".spf2";
                files.Add(new PresetFile { Name = name, Preset = preset });
            }
            return files;
        }

        public Preset GeneratePreset(int index, string type, string author, ArpConfig arp)
        {
            string t = NormalizeType(type);
            var preset = GetPresetTemplate();
            preset.author = author;
            preset.tags.Add(ToTypeName(t));

            string notes = "Generated preset " + (index + 1) + "\nType: " + ToTypeName(t) + "\nGenerated: " + DateTime.Now.ToString(CultureInfo.CurrentCulture);
            if (arp != null && arp.Enabled)
            {
                notes += "\nArpeggiator: ON";
                if (!string.IsNullOrEmpty(arp.ModeName)) notes += "\nMode: " + arp.ModeName;
                if (!string.IsNullOrEmpty(arp.SpeedName)) notes += "\nSpeed: " + arp.SpeedName;
                notes += "\nPattern: " + (arp.Pattern ?? "random");
            }
            preset.notes = notes;

            var parameters = GenerateParameters(t);

            // Применяем арпеджиатор ко всем типам пресетов, если он включен
            if (arp != null && arp.Enabled)
            {
                parameters["arp_on"] = 1;
                parameters["arp_mode"] = Clamp01(arp.Mode);
                parameters["arp_octave"] = Clamp01(arp.Octave);
                parameters["arp_speed"] = Clamp01(arp.Speed);
                // Улучшенные параметры для более мелодичной игры в Spire
                parameters["arp_swing"] = RandomInRange(0.48, 0.58); // более выраженный swing
                parameters["arp_length"] = RandomInRange(0.5, 0.9); // более длинные ноты
                parameters["arp_velocity"] = RandomInRange(0.4, 0.8); // более выразительная динамика
                parameters["arp_gate"] = RandomInRange(0.6, 0.9); // добавляем gate для Spire
                parameters["arp_retrigger"] = RandomInRange(0.1, 0.3); // добавляем retrigger

                // Специальная обработка для режима Step
                if (Math.Abs(arp.Mode - 1.0) < 0.01) // режим Step
                {
                    // Устанавливаем специальные параметры для режима Step с большей вариацией
                    double[] modeValues = new[] { 0.001001, 0.00800801, 0.003003, 0.005005 };
                    parameters["arp_mode"] = modeValues[RandomInt(0, modeValues.Length - 1)];
                    parameters["arp_wrap"] = RandomInRange(0.002, 0.020); // расширенный диапазон

                    // Больше вариаций velocity
                    double[] velocityValues = new[] { 0, 0.001001, 0.002002, 0.003003, 0.004004 };
                    parameters["arp_velocity"] = velocityValues[RandomInt(0, velocityValues.Length - 1)];
                    
                    // Генерируем пошаговый паттерн
                    var stepPattern = GenerateStepPattern(16, arp);
                    for (int i = 1; i <= 16; i++)
                    {
                        parameters["arp_note" + i] = stepPattern.notes[i - 1];
                        parameters["arp_vel" + i] = stepPattern.velocities[i - 1];
                        parameters["arp_hold" + i] = stepPattern.holds[i - 1];
                    }

                    // Настройка степперов для Step режима
                    for (int i = 1; i <= 16; i++)
                    {
                        parameters["stp1_start" + i] = RandomInRange(0, 1);
                        parameters["stp1_end" + i] = RandomInRange(0, 1);
                        parameters["stp1_crvX" + i] = RandomInRange(0, 1);
                        parameters["stp1_crvY" + i] = RandomInRange(0, 1);
                        parameters["stp1_hold" + i] = RandomInRange(0, 1);
                        parameters["stp1_rep" + i] = RandomInRange(0, 1);
                    }
                    parameters["stp1_rate"] = RandomInRange(0.3, 0.7);
                    parameters["stp1_time"] = RandomInRange(0.005, 0.02);
                    parameters["stp1_mode"] = RandomInRange(0.001, 0.01);
                    parameters["stp1_start"] = 0;
                    parameters["stp1_stop"] = RandomInRange(0.01, 0.025);
                    parameters["stp1_mono"] = 0;
                    parameters["stp1_rtrg"] = 1;
                    parameters["stp1_loop"] = 1;

                    // Второй степпер для дополнительной модуляции
                    for (int i = 1; i <= 16; i++)
                    {
                        parameters["stp2_start" + i] = RandomInRange(0, 1);
                        parameters["stp2_end" + i] = RandomInRange(0, 1);
                        parameters["stp2_crvX" + i] = RandomInRange(0, 1);
                        parameters["stp2_crvY" + i] = RandomInRange(0, 1);
                        parameters["stp2_hold" + i] = RandomInRange(0, 1);
                        parameters["stp2_rep" + i] = RandomInRange(0, 1);
                    }
                    parameters["stp2_rate"] = RandomInRange(0.2, 0.6);
                    parameters["stp2_time"] = RandomInRange(0.008, 0.025);
                    parameters["stp2_mode"] = RandomInRange(0.001, 0.008);
                    parameters["stp2_start"] = 0;
                    parameters["stp2_stop"] = RandomInRange(0.015, 0.03);
                    parameters["stp2_mono"] = 0;
                    parameters["stp2_rtrg"] = 1;
                    parameters["stp2_loop"] = 1;
                }
                else
                {
                    // Обычный арпеджиатор для всех типов
                    string arpPatternType = arp.Pattern ?? "melody";
                    if (arpPatternType == "random")
                    {
                        var patterns = new[] { "melody", "chord", "rhythmic", "bass" };
                        arpPatternType = patterns[RandomInt(0, patterns.Length - 1)];
                    }
                    var arpPattern = GenerateArpeggio(arpPatternType, 16, arp);
                    for (int i = 1; i <= 16; i++)
                    {
                        parameters["arp_note" + i] = arpPattern.notes[i - 1];
                        parameters["arp_vel" + i] = arpPattern.velocities[i - 1];
                        parameters["arp_hold" + i] = arpPattern.holds[i - 1];
                    }
                }
            }

            if (string.Equals(t, "sequence", StringComparison.OrdinalIgnoreCase))
            {
                // Дополнительные настройки для sequence типа (арпеджиатор уже применен выше)

                // Два пошаговых модуляторов (stepper) для ритмики и тону
                int sequenceStepperType = RandomInt(0, 3); // выбираем тип паттерна для всех степперов
                for (int s = 1; s <= 2; s++)
                {
                    parameters["stp" + s + "_rate"] = RandomInRange(0.40, 0.60); // больше вариаций
                    parameters["stp" + s + "_time"] = RandomInRange(0.010, 0.016); // добавили вариацию
                    parameters["stp" + s + "_mode"] = (_rnd.NextDouble() < 0.7) ? 0.002 : 0.001; // curve или step
                    parameters["stp" + s + "_start"] = 0;
                    parameters["stp" + s + "_stop"] = RandomInRange(0.010, 0.020); // добавили вариацию
                    parameters["stp" + s + "_mono"] = 0;
                    parameters["stp" + s + "_rtrg"] = 1;
                    parameters["stp" + s + "_loop"] = 1;

                    for (int i = 1; i <= 16; i++)
                    {
                        double phase = (i - 1) / 15.0;
                        bool accent;
                        double shape;
                        
                        if (sequenceStepperType == 0) // волнообразный с акцентами на кратных 4
                        {
                            shape = 0.5 + Math.Sin(phase * Math.PI * 2) * RandomInRange(0.25, 0.50) * (s == 1 ? 1.0 : -1.0);
                            accent = (i % 4) == 1;
                        }
                        else if (sequenceStepperType == 1) // двойная волна
                        {
                            shape = 0.5 + Math.Sin(phase * Math.PI * 4) * RandomInRange(0.20, 0.45) * (s == 1 ? 1.0 : -1.0);
                            accent = (i % 2) == 0;
                        }
                        else if (sequenceStepperType == 2) // восходящий/нисходящий
                        {
                            bool ascending = s == 1;
                            shape = ascending ? (phase * RandomInRange(0.7, 1.0)) : ((1.0 - phase) * RandomInRange(0.7, 1.0));
                            accent = (i % 8) == 0 || (i % 8) == 4;
                        }
                        else // случайные значения с редкими акцентами
                        {
                            shape = RandomInRange(0.2, 0.8);
                            accent = _rnd.NextDouble() < 0.25;
                        }
                        
                        parameters["stp" + s + "_start" + i] = Math.Max(0, Math.Min(1, shape));
                        parameters["stp" + s + "_end" + i] = accent ? RandomInRange(0.05, 0.20) : RandomInRange(0.0, 0.12);
                        parameters["stp" + s + "_hold" + i] = accent ? (_rnd.NextDouble() < 0.7 ? 1 : 0) : 0;
                        parameters["stp" + s + "_rep" + i] = 0;
                        parameters["stp" + s + "_crvX" + i] = RandomInRange(0.35, 0.65);
                        parameters["stp" + s + "_crvY" + i] = RandomInRange(0.35, 0.65);
                    }
                }

                // Корректировки огибающих под секвенции
                parameters["env1_att"] = RandomInRange(0, 0.01);
                parameters["env1_dec"] = RandomInRange(0.15, 0.35);
                parameters["env1_sus"] = RandomInRange(0.5, 0.8);
                parameters["env1_rel"] = RandomInRange(0.15, 0.35);
                parameters["glide"] = RandomInRange(0.05, 0.2);
            }
            else if (string.Equals(t, "atmo", StringComparison.OrdinalIgnoreCase))
            {
                // Атмосферные пресеты: эмбиент пады с длинными огибающими и эффектами
                parameters["rev_wet"] = RandomInRange(0.4, 0.8); // больше реверба
                parameters["del_wet"] = RandomInRange(0.2, 0.5); // задержка
                parameters["chr_wet"] = RandomInRange(0.1, 0.3); // хорус
                parameters["phs_wet"] = RandomInRange(0.1, 0.2); // фейзер
                parameters["lfo1_rate"] = RandomInRange(0.1, 0.3); // медленная модуляция
                parameters["lfo1_target1"] = 0.01; // модуляция фильтра
                parameters["lfo1_amt1"] = RandomInRange(0.2, 0.4);
            }
            else if (string.Equals(t, "key", StringComparison.OrdinalIgnoreCase))
            {
                // Пианино: чистый звук с быстрой атакой и естественным decay
                parameters["rev_wet"] = RandomInRange(0.1, 0.3); // немного реверба
                parameters["del_wet"] = RandomInRange(0.0, 0.1); // минимальная задержка
                parameters["chr_wet"] = 0; // без хоруса
                parameters["phs_wet"] = 0; // без фейзера
                parameters["shp_wet"] = 0; // без дисторшена
                parameters["velocity_sens"] = RandomInRange(0.6, 0.9); // чувствительность к velocity
            }
            else if (string.Equals(t, "gt", StringComparison.OrdinalIgnoreCase))
            {
                // Гейтированные: короткие ноты с резкими огибающими
                parameters["env1_att"] = 0; // мгновенная атака
                parameters["env1_dec"] = RandomInRange(0.05, 0.15); // быстрый decay
                parameters["env1_sus"] = RandomInRange(0.1, 0.3); // низкий sustain
                parameters["env1_rel"] = RandomInRange(0.02, 0.1); // быстрый release
                parameters["lfo1_rate"] = RandomInRange(0.2, 0.6); // быстрая модуляция для ритма
                parameters["lfo1_target1"] = 0.005; // модуляция громкости
                parameters["lfo1_amt1"] = RandomInRange(0.3, 0.6);
            }
            else if (string.Equals(t, "sy", StringComparison.OrdinalIgnoreCase))
            {
                // Синт эмуляции: классические аналоговые настройки
                parameters["osc1_wave"] = RandomInRange(0, 0.3); // пила/квадрат
                parameters["osc2_wave"] = RandomInRange(0, 0.3);
                parameters["osc2_tune"] = RandomInRange(-0.02, 0.02); // небольшая расстройка
                parameters["osc2_level"] = RandomInRange(0.3, 0.7); // смешивание осцилляторов
                parameters["lfo1_rate"] = RandomInRange(0.3, 0.7); // классическая модуляция
                parameters["lfo1_target1"] = RandomInRange(0.01, 0.02); // модуляция фильтра
                parameters["lfo1_amt1"] = RandomInRange(0.4, 0.8);
                parameters["chr_wet"] = RandomInRange(0.1, 0.2); // легкий хорус
            }
            else if (string.Equals(t, "pluck", StringComparison.OrdinalIgnoreCase))
            {
                // Плаки: быстрая атака, короткий релиз, умеренные FX
                parameters["env1_att"] = RandomInRange(0.0, 0.01);
                parameters["env1_dec"] = RandomInRange(0.15, 0.4);
                parameters["env1_sus"] = 0;
                parameters["env1_rel"] = RandomInRange(0.05, 0.2);

                // Фильтр и акцент огибающей фильтра
                parameters["flt1_type"] = 0; // LP
                parameters["flt1_res"] = RandomInRange(0.2, 0.5);
                parameters["env2_att"] = 0;
                parameters["env2_dec"] = RandomInRange(0.15, 0.4);
                parameters["env2_sus"] = 0;
                parameters["env2_rel"] = RandomInRange(0.08, 0.22);
                parameters["env2_target1"] = RandomInRange(0.038, 0.05);
                parameters["env2_amt1"] = RandomInRange(0.5, 0.8);

                // FX: умеренная задержка и реверберация для «вкусности» без хвоста
                parameters["del_wet"] = RandomInRange(0.1, 0.35);
                parameters["rev_wet"] = RandomInRange(0.1, 0.35);
                parameters["chr_wet"] = RandomInRange(0.0, 0.1);
                parameters["phs_wet"] = 0;

                // Яркий тон
                parameters["mix_osc1"] = RandomInRange(0.6, 0.9);
                parameters["mix_osc2"] = RandomInRange(0.0, 0.3);
                parameters["osc1_wave"] = RandomInRange(0.0, 0.25); // пила/яркие
                parameters["osc2_wave"] = RandomInRange(0.0, 0.5);
                parameters["velocity_sens"] = RandomInRange(0.6, 0.9);
            }
            else if (string.Equals(t, "lead", StringComparison.OrdinalIgnoreCase))
            {
                // Лиды: по требованию — короткие, ударные (не тянущиеся)
                parameters["env1_att"] = RandomInRange(0.0, 0.01);
                parameters["env1_dec"] = RandomInRange(0.1, 0.35);
                parameters["env1_sus"] = RandomInRange(0.0, 0.2);
                parameters["env1_rel"] = RandomInRange(0.05, 0.2);

                // Фильтр и модуляция среза огибающей
                parameters["flt1_type"] = 0;
                parameters["flt1_res"] = RandomInRange(0.25, 0.55);
                parameters["env2_att"] = 0;
                parameters["env2_dec"] = RandomInRange(0.12, 0.35);
                parameters["env2_sus"] = 0;
                parameters["env2_rel"] = RandomInRange(0.08, 0.2);
                parameters["env2_target1"] = RandomInRange(0.04, 0.05);
                parameters["env2_amt1"] = RandomInRange(0.5, 0.85);

                // FX: минимум хвостов, чуть задержки для ширины
                parameters["del_wet"] = RandomInRange(0.05, 0.25);
                parameters["rev_wet"] = RandomInRange(0.05, 0.2);
                parameters["chr_wet"] = RandomInRange(0.0, 0.1);
                parameters["phs_wet"] = 0;

                // Осцилляторы: ярко и плотнее по сравнению с pluck
                parameters["mix_osc1"] = RandomInRange(0.7, 0.95);
                parameters["mix_osc2"] = RandomInRange(0.2, 0.5);
                parameters["osc1_wave"] = RandomInRange(0.0, 0.35);
                parameters["osc2_wave"] = RandomInRange(0.0, 0.6);
                parameters["velocity_sens"] = RandomInRange(0.4, 0.7);
            }
            else if (string.Equals(t, "bass", StringComparison.OrdinalIgnoreCase))
            {
                // Октава баса: только -4 / -3 / -2
                double[] bassOcts = new double[] { 0.0, 0.125, 0.25 }; // -4, -3, -2
                double bassOct = bassOcts[RandomInt(0, bassOcts.Length - 1)];
                parameters["osc1_oct"] = bassOct;
                parameters["osc2_oct"] = bassOct;
                parameters["osc3_oct"] = bassOct;
                parameters["osc4_oct"] = bassOct;
                bool isDeep = _rnd.NextDouble() < 0.5; // deep sustain vs short punch

                if (isDeep)
                {
                    // Глубокий протяжный бас: высокая выдержка, плавность, возможный глайд
                    parameters["env1_att"] = RandomInRange(0.0, 0.02);
                    parameters["env1_dec"] = RandomInRange(0.2, 0.6);
                    parameters["env1_sus"] = RandomInRange(0.7, 1.0);
                    parameters["env1_rel"] = RandomInRange(0.1, 0.35);

                    parameters["flt1_type"] = 0; // LP
                    parameters["flt1_res"] = RandomInRange(0.2, 0.45);
                    parameters["env2_att"] = 0;
                    parameters["env2_dec"] = RandomInRange(0.2, 0.5);
                    parameters["env2_sus"] = 0;
                    parameters["env2_rel"] = RandomInRange(0.1, 0.3);
                    parameters["env2_target1"] = RandomInRange(0.04, 0.05);
                    parameters["env2_amt1"] = RandomInRange(0.5, 0.85);

                    parameters["glide"] = (_rnd.NextDouble() < 0.35) ? RandomInRange(0.3, 0.6) : 0;

                    // Осцилляторы: больше основного тона, минимум овердрайва
                    parameters["mix_osc1"] = RandomInRange(0.75, 0.95);
                    parameters["mix_osc2"] = RandomInRange(0.0, 0.25);
                    parameters["osc1_wave"] = RandomInRange(0.0, 0.3); // пила/квадрат в нижнем диапазоне
                    parameters["osc2_wave"] = RandomInRange(0.0, 0.5);

                    // FX: мало хвостов, слегка задержка; ревёрб почти нет
                    parameters["del_wet"] = RandomInRange(0.05, 0.2);
                    parameters["rev_wet"] = RandomInRange(0.0, 0.15);
                    parameters["shp_mode"] = RandomInRange(0.001, 0.004);
                    parameters["shp_drive"] = RandomInRange(0.2, 0.45);
                    parameters["shp_wet"] = RandomInRange(0.08, 0.2);

					// Осцилляторы (тембр): субовый акцент, минимум унисона
					parameters["mix_osc1"] = RandomInRange(0.85, 1.0);
					parameters["mix_osc2"] = RandomInRange(0.0, 0.2);
					parameters["osc1_wave"] = RandomInRange(0.0, 0.03);
					parameters["osc2_wave"] = 0.0;
					parameters["osc1_wtmix"] = RandomInRange(0.0, 0.2);
					parameters["osc2_wtmix"] = 0.0;
					parameters["osc1_udet"] = RandomInRange(0.0, 0.2);
					parameters["osc1_uwid"] = RandomInRange(0.0, 0.1);
					parameters["osc1_ucnt"] = 0.0;
					parameters["osc2_udet"] = RandomInRange(0.0, 0.15);
					parameters["osc2_uwid"] = RandomInRange(0.0, 0.1);
					parameters["osc2_ucnt"] = 0.0;
                }
                else
                {
                    // Короткий гудящий/панчевый бас: низкий sustain, быстрый релиз, акцент фильтра
                    parameters["env1_att"] = RandomInRange(0.0, 0.01);
                    parameters["env1_dec"] = RandomInRange(0.1, 0.35);
                    parameters["env1_sus"] = RandomInRange(0.0, 0.2);
                    parameters["env1_rel"] = RandomInRange(0.02, 0.15);

                    parameters["flt1_type"] = 0;
                    parameters["flt1_res"] = RandomInRange(0.3, 0.6);
                    parameters["env2_att"] = 0;
                    parameters["env2_dec"] = RandomInRange(0.12, 0.35);
                    parameters["env2_sus"] = 0;
                    parameters["env2_rel"] = RandomInRange(0.08, 0.2);
                    parameters["env2_target1"] = RandomInRange(0.04, 0.05);
                    parameters["env2_amt1"] = RandomInRange(0.6, 0.95);

                    parameters["glide"] = (_rnd.NextDouble() < 0.2) ? RandomInRange(0.2, 0.5) : 0;

                    // Осцилляторы и сатурация: чуть плотнее
                    parameters["mix_osc1"] = RandomInRange(0.75, 0.95);
                    parameters["mix_osc2"] = RandomInRange(0.2, 0.6);
                    parameters["osc1_wave"] = RandomInRange(0.0, 0.06);
                    parameters["osc2_wave"] = RandomInRange(0.0, 0.05);
                    parameters["osc1_wtmix"] = RandomInRange(0.2, 1.0);
                    parameters["osc2_wtmix"] = RandomInRange(0.0, 1.0);
                    parameters["osc1_udet"] = RandomInRange(0.3, 0.7);
                    parameters["osc1_uwid"] = RandomInRange(0.3, 1.0);
                    parameters["osc1_ucnt"] = RandomInRange(0.0, 0.01);
                    parameters["osc2_udet"] = RandomInRange(0.2, 0.6);
                    parameters["osc2_uwid"] = RandomInRange(0.2, 0.9);
                    parameters["osc2_ucnt"] = RandomInRange(0.0, 0.01);
                    parameters["shp_mode"] = RandomInRange(0.002, 0.006);
                    parameters["shp_drive"] = RandomInRange(0.35, 0.7);
                    parameters["shp_wet"] = RandomInRange(0.12, 0.3);

                    // FX: минимально, чтобы сохранить ударность
                    parameters["del_wet"] = RandomInRange(0.03, 0.15);
                    parameters["rev_wet"] = RandomInRange(0.0, 0.12);
                }
            }
            else if (string.Equals(t, "fx", StringComparison.OrdinalIgnoreCase))
            {
                // FX генерация: подтипы на основе реальных пресетов
                var fxTypes = new[] { "riser", "down", "impact", "sweep", "whoosh", "glitch" };
                string fx = fxTypes[RandomInt(0, fxTypes.Length - 1)];

                // База FX: широкие диапазоны огибающих/модуляции/FX
                parameters["mix_osc1"] = RandomInRange(0.4, 0.9);
                parameters["mix_osc2"] = RandomInRange(0.0, 0.6);
                parameters["osc1_wave"] = RandomInRange(0.0, 1.0);
                parameters["osc2_wave"] = RandomInRange(0.0, 1.0);
                parameters["lfo1_rate"] = RandomInRange(0.0, 1.0);
                parameters["env2_target1"] = RandomInRange(0.0, 0.08);
                parameters["env2_amt1"] = RandomInRange(0.45, 1.0);

                if (fx == "riser")
                {
                    parameters["env1_att"] = RandomInRange(0.3, 0.8);
                    parameters["env1_dec"] = RandomInRange(0.2, 0.5);
                    parameters["env1_sus"] = RandomInRange(0.2, 0.7);
                    parameters["env1_rel"] = RandomInRange(0.2, 0.5);
                    parameters["phs_wet"] = RandomInRange(0.3, 1.0);
                    parameters["chr_wet"] = RandomInRange(0.0, 0.35);
                    parameters["del_wet"] = RandomInRange(0.1, 0.6);
                    parameters["rev_wet"] = RandomInRange(0.2, 0.6);
                }
                else if (fx == "down")
                {
                    parameters["env1_att"] = RandomInRange(0.05, 0.3);
                    parameters["env1_dec"] = RandomInRange(0.2, 0.6);
                    parameters["env1_sus"] = RandomInRange(0.0, 0.4);
                    parameters["env1_rel"] = RandomInRange(0.2, 0.6);
                    parameters["phs_wet"] = RandomInRange(0.2, 0.8);
                    parameters["chr_wet"] = RandomInRange(0.0, 0.3);
                    parameters["del_wet"] = RandomInRange(0.1, 0.55);
                    parameters["rev_wet"] = RandomInRange(0.15, 0.55);
                }
                else if (fx == "impact")
                {
                    parameters["env1_att"] = RandomInRange(0.0, 0.02);
                    parameters["env1_dec"] = RandomInRange(0.05, 0.25);
                    parameters["env1_sus"] = 0;
                    parameters["env1_rel"] = RandomInRange(0.1, 0.35);
                    parameters["del_wet"] = RandomInRange(0.1, 0.4);
                    parameters["rev_wet"] = RandomInRange(0.45, 0.9);
                    parameters["phs_wet"] = RandomInRange(0.0, 0.4);
                    parameters["chr_wet"] = RandomInRange(0.0, 0.25);
                }
                else if (fx == "sweep")
                {
                    parameters["env1_att"] = RandomInRange(0.15, 0.5);
                    parameters["env1_dec"] = RandomInRange(0.2, 0.5);
                    parameters["env1_sus"] = RandomInRange(0.0, 0.4);
                    parameters["env1_rel"] = RandomInRange(0.2, 0.5);
                    parameters["phs_wet"] = RandomInRange(0.5, 1.0);
                    parameters["chr_wet"] = RandomInRange(0.0, 0.35);
                    parameters["del_wet"] = RandomInRange(0.0, 0.4);
                    parameters["rev_wet"] = RandomInRange(0.1, 0.5);
                }
                else if (fx == "whoosh")
                {
                    parameters["env1_att"] = RandomInRange(0.2, 0.7);
                    parameters["env1_dec"] = RandomInRange(0.2, 0.6);
                    parameters["env1_sus"] = RandomInRange(0.2, 0.7);
                    parameters["env1_rel"] = RandomInRange(0.2, 0.6);
                    parameters["phs_wet"] = RandomInRange(0.2, 0.7);
                    parameters["chr_wet"] = RandomInRange(0.0, 0.5);
                    parameters["del_wet"] = RandomInRange(0.1, 0.7);
                    parameters["rev_wet"] = RandomInRange(0.2, 0.85);
                }
                else // glitch
                {
                    parameters["env1_att"] = RandomInRange(0.0, 0.05);
                    parameters["env1_dec"] = RandomInRange(0.05, 0.25);
                    parameters["env1_sus"] = RandomInRange(0.0, 0.2);
                    parameters["env1_rel"] = RandomInRange(0.02, 0.2);
                    parameters["del_wet"] = RandomInRange(0.0, 0.35);
                    parameters["rev_wet"] = RandomInRange(0.0, 0.35);
                    // Степпер для строба/гейта
                    for (int i = 1; i <= 16; i++) { parameters["stp1_start" + i] = (i % 2 == 0) ? 1 : 0; parameters["stp1_end" + i] = (i % 2 == 0) ? 0 : 1; parameters["stp1_crvX" + i] = 0.5; parameters["stp1_crvY" + i] = 0.5; parameters["stp1_hold" + i] = 0; parameters["stp1_rep" + i] = 0; }
                    parameters["stp1_rate"] = 0.5; parameters["stp1_time"] = 0.01; parameters["stp1_mode"] = 0.001; parameters["stp1_start"] = 0; parameters["stp1_stop"] = 0.015015; parameters["stp1_mono"] = 0; parameters["stp1_rtrg"] = 1; parameters["stp1_loop"] = 1;
                }
            }
            
            // Фиксация cutoff-ручек на 85% для ВСЕХ типов пресетов
            parameters["flt1_cut"] = 0.85;
            parameters["flt2_cut"] = 0.85;
            
            // Фиксация громкости осцилляторов на 100% только для обычных типов (не sequence, не Step-арпеджиатор)
            bool isSequence = string.Equals(t, "sequence", StringComparison.OrdinalIgnoreCase);
            bool isStepArp = (arp != null && arp.Enabled && Math.Abs(arp.Mode - 1.0) < 0.01);
            
            if (!isSequence && !isStepArp)
            {
                parameters["mix_osc1"] = 1.0;
                parameters["mix_osc2"] = 1.0;
                parameters["mix_osc3"] = 1.0;
                parameters["mix_osc4"] = 1.0;
                parameters["osc1_level"] = 1.0;
                parameters["osc2_level"] = 1.0;
                parameters["osc3_level"] = 1.0;
                parameters["osc4_level"] = 1.0;
            }
            
            preset.parameters = parameters;
            return preset;
        }

        private static string NormalizeType(string type)
        {
            switch ((type ?? "lead").ToLowerInvariant())
            {
                case "lead": case "pad": case "bass": case "pluck": case "fx": case "sequence": case "atmo": case "key": case "gt": case "sy": return type.ToLowerInvariant();
                default: return "lead"; // по умолчанию lead вместо random
            }
        }

        private static string ToTypeName(string t)
        {
            if (t == "lead") return "Lead"; if (t == "pad") return "Pad"; if (t == "bass") return "Bass"; if (t == "pluck") return "Pluck"; if (t == "fx") return "FX"; if (t == "sequence") return "Seq"; if (t == "atmo") return "Atmo"; if (t == "key") return "Key"; if (t == "gt") return "Gate"; if (t == "sy") return "Synth"; return "Lead";
        }

        private Preset GetPresetTemplate()
        {
            return new Preset
            {
                icon = string.Empty,
                vendor = string.Empty,
                favorite = "no",
                bank = string.Empty,
                tags = new List<string>(),
                author = string.Empty,
                notes = string.Empty,
                parameters = new Dictionary<string, double>()
            };
        }

        private Dictionary<string, double> GenerateParameters(string type)
        {
            var p = new Dictionary<string, double>();

            for (int i = 1; i <= 4; i++)
            {
                p["osc" + i + "_type"] = RandomInRange(0, 0.004);
                p["osc" + i + "_oct"] = RandomInRange(0.25, 0.75);
                p["osc" + i + "_note"] = 0.5;
                p["osc" + i + "_fine"] = RandomInRange(0.45, 0.55);
                p["osc" + i + "_ctrlA"] = Random01();
                p["osc" + i + "_ctrlB"] = Random01();
                p["osc" + i + "_phase"] = Random01();
                p["osc" + i + "_wave"] = Random01();
                p["osc" + i + "_wtmix"] = Random01();
                p["osc" + i + "_udet"] = RandomInRange(0.4, 0.7);
                p["osc" + i + "_uwid"] = Random01();
                p["osc" + i + "_ucnt"] = RandomInRange(0, 0.02);
                p["osc" + i + "_uoct"] = 0;
                p["osc" + i + "_uden"] = 0.5;
                p["osc" + i + "_pan"] = RandomInRange(0.3, 0.7);
                p["osc" + i + "_ana"] = 1;
                p["osc" + i + "_inv"] = 0;
                p["osc" + i + "_key"] = 1;
                p["osc" + i + "_mixto"] = 0;
                p["osc" + i + "_mute"] = 0;
            }

            p["mix_osc1"] = RandomInRange(0.4, 0.8);
            p["mix_osc2"] = RandomInRange(0.3, 0.7);
            p["mix_osc3"] = RandomInRange(0, 0.5);
            p["mix_osc4"] = RandomInRange(0, 0.5);

            var flt = GetFilterSettings(type);
            p["flt1_type"] = RandomInRange(0, 0.01);
            p["flt1_mode"] = 0;
            p["flt1_cut"] = flt.cut;
            p["flt1_res"] = flt.res;
            p["flt2_type"] = 0;
            p["flt2_mode"] = RandomInRange(-0.001, 0.001);
            p["flt2_cut"] = 0.85;
            p["flt2_res"] = 0;
            p["flt_rout"] = 0;
            p["flt_link"] = 0;
            p["flt_balance"] = 1;
            p["flt_keytrack"] = 0.5;

            for (int i = 1; i <= 4; i++)
            {
                var env = (i == 1 || i == 2) ? GetEnvSettings(type) : new EnvVals { att = 0, dec = 0, sus = 1, rel = 0 };
                p["env" + i + "_att"] = env.att;
                p["env" + i + "_dec"] = env.dec;
                p["env" + i + "_sus"] = env.sus;
                p["env" + i + "_slt"] = 0;
                p["env" + i + "_sll"] = 0;
                p["env" + i + "_rel"] = env.rel;
                p["env" + i + "_att_crv"] = 0;
                p["env" + i + "_dec_crv"] = 1;
                p["env" + i + "_slp_crv"] = (i == 2) ? 1 : 0;
                p["env" + i + "_rel_crv"] = 0;
                p["env" + i + "_target1"] = (i == 2) ? RandomInRange(0.04, 0.05) : 0;
                p["env" + i + "_amt1"] = (i == 2) ? RandomInRange(0.6, 0.9) : 0.5;
                p["env" + i + "_vel1"] = 0.5;
                p["env" + i + "_target2"] = 0;
                p["env" + i + "_amt2"] = 0.5;
                p["env" + i + "_vel2"] = 0.5;
            }

            for (int i = 1; i <= 4; i++)
            {
                p["lfo" + i + "_rate"] = RandomInRange(0.5, 0.8);
                p["lfo" + i + "_time"] = 0.01;
                p["lfo" + i + "_sym"] = 0.5;
                p["lfo" + i + "_sync"] = 0;
                p["lfo" + i + "_mono"] = 0;
                p["lfo" + i + "_morph"] = 0.5;
                p["lfo" + i + "_phase"] = 0;
                p["lfo" + i + "_form"] = 0;
                p["lfo" + i + "_amp"] = 0.5;
                p["lfo" + i + "_fadein"] = 0;
                p["lfo" + i + "_target1"] = (i == 1) ? RandomInRange(0.01, 0.02) : 0;
                p["lfo" + i + "_amt1"] = (i == 1) ? RandomInRange(0.5, 0.8) : 0.5;
                p["lfo" + i + "_vel1"] = 0.5;
                p["lfo" + i + "_target2"] = 0;
                p["lfo" + i + "_amt2"] = 0.5;
                p["lfo" + i + "_vel2"] = 0.5;
            }

            p["shp_mode"] = RandomInRange(0, 0.01);
            p["shp_band"] = 0;
            p["shp_drive"] = RandomInRange(0, 0.3);
            p["shp_bit"] = 1;
            p["shp_sr"] = 1;
            p["shp_hicut"] = 1;
            p["shp_lowcut"] = 0;
            p["shp_wet"] = RandomInRange(0, 0.2);
            p["shp_hq"] = 1;
            p["shp_mute"] = 0;

            p["phs_stages"] = 0.003;
            p["phs_freq"] = 0;
            p["phs_fback"] = 0.55;
            p["phs_spread"] = 0.5;
            p["phs_rate"] = 0.1;
            p["phs_depth"] = 0.9;
            p["phs_wet"] = RandomInRange(0, 0.3);
            p["phs_pre"] = 0;
            p["phs_mute"] = 0;

            p["chr_delay"] = 0.5;
            p["chr_mode"] = 0;
            p["chr_fback"] = 0.5;
            p["chr_rate"] = 0.5;
            p["chr_depth"] = 0.5;
            p["chr_wide"] = 0.5;
            p["chr_hicut"] = 1;
            p["chr_lowcut"] = 0;
            p["chr_wet"] = RandomInRange(0, 0.2);
            p["chr_mute"] = 0;

            p["del_timeL"] = 0.4;
            p["del_timeR"] = 0.5;
            p["del_stimeL"] = 0.007;
            p["del_stimeR"] = 0.007;
            p["del_pingpong"] = 1;
            p["del_sync"] = 1;
            p["del_rate"] = 0.2;
            p["del_fback"] = RandomInRange(0.2, 0.6);
            p["del_wide"] = 0.5;
            p["del_modulate"] = 0.05;
            p["del_color"] = 0.5;
            p["del_wet"] = RandomInRange(0.1, 0.4);
            p["del_mute"] = 0;

            p["rev_mode"] = 0;
            p["rev_sync"] = 0;
            p["rev_pretime"] = 0;
            p["rev_pretimeS"] = 0.007;
            p["rev_damp"] = RandomInRange(0.1, 0.3);
            p["rev_wide"] = 1;
            p["rev_decay"] = RandomInRange(0.1, 0.4);
            p["rev_color"] = RandomInRange(0.5, 0.9);
            p["rev_wet"] = RandomInRange(0.2, 0.5);
            p["rev_mute"] = 0;

            p["eq_band"] = 1;
            p["eq_power"] = 1;
            p["eq_low_frq"] = 0.2;
            p["eq_low_Q"] = 0.54;
            p["eq_low_lev"] = RandomInRange(0.3, 0.6);
            p["eq_mid_frq"] = 0.5;
            p["eq_mid_Q"] = 0.45;
            p["eq_mid_lev"] = RandomInRange(0.4, 0.7);
            p["eq_hi_frq"] = 0.53;
            p["eq_hi_Q"] = 0.4;
            p["eq_hi_lev"] = RandomInRange(0.5, 0.8);

            p["mode"] = 0.004;
            p["poly"] = 0.003;
            p["bend_up"] = 0.002;
            p["bend_dn"] = 0.002;
            p["glide_log"] = 1;
            p["glide"] = RandomInRange(0.2, 0.4);
            p["drift"] = 1;
            p["x-comp"] = 0;
            p["velocity"] = 0.5;
            p["volume"] = 1.0;
            p["warm"] = 1;
            p["boost"] = 0;
            p["soft"] = 0;
            p["anticlick"] = 1;
            p["mod_wheel"] = 0;
            p["pitch_shift"] = 0.5;
            p["pitch_fine"] = 0.5;

            for (int i = 1; i <= 4; i++) p["macro" + i] = 0;

            for (int i = 1; i <= 15; i++)
            {
                p["slot" + i + "_Src1"] = (i == 1) ? RandomInRange(0.01, 0.02) : 0;
                p["slot" + i + "_Src2"] = 0;
                p["slot" + i + "_Amt1"] = (i == 1) ? RandomInRange(0.4, 0.7) : 0.5;
                p["slot" + i + "_Trg1"] = (i == 1) ? RandomInRange(0.04, 0.05) : 0;
                p["slot" + i + "_Amt2"] = (i == 1) ? RandomInRange(0.5, 0.7) : 0.5;
                p["slot" + i + "_Trg2"] = 0;
                p["slot" + i + "_Amt3"] = 0.5;
                p["slot" + i + "_Trg3"] = 0;
                p["slot" + i + "_Amt4"] = 0.5;
                p["slot" + i + "_Trg4"] = 0;
            }

            p["arp_on"] = 0;
            p["arp_mode"] = 0;
            p["arp_octave"] = 0;
            p["arp_velocity"] = 0;
            p["arp_swing"] = 0.5;
            p["arp_speed"] = 0.016;
            p["arp_length"] = 0.5;
            p["arp_wrap"] = 0.016;
            for (int i = 1; i <= 16; i++) { p["arp_note" + i] = 0.5; p["arp_vel" + i] = 0.78; p["arp_hold" + i] = 0; }

            for (int s = 1; s <= 2; s++)
            {
                p["stp" + s + "_rate"] = 0.5;
                p["stp" + s + "_time"] = 0.013;
                p["stp" + s + "_mode"] = 0.002;
                p["stp" + s + "_start"] = 0;
                p["stp" + s + "_stop"] = 0.015;
                p["stp" + s + "_mono"] = 0;
                p["stp" + s + "_rtrg"] = 1;
                p["stp" + s + "_loop"] = 1;
                for (int i = 1; i <= 16; i++)
                {
                    p["stp" + s + "_start" + i] = 1;
                    p["stp" + s + "_end" + i] = 0;
                    p["stp" + s + "_hold" + i] = 0;
                    p["stp" + s + "_rep" + i] = 0;
                    p["stp" + s + "_crvX" + i] = 0.5;
                    p["stp" + s + "_crvY" + i] = 0.5;
                }
            }

            return p;
        }

        private (double cut, double res) GetFilterSettings(string type)
        {
            switch (type)
            {
                case "lead": return (RandomInRange(0.3, 0.6), RandomInRange(0.1, 0.4));
                case "pad": return (RandomInRange(0.2, 0.5), RandomInRange(0.0, 0.2));
                case "bass": return (RandomInRange(0.2, 0.4), RandomInRange(0.2, 0.5));
                case "pluck": return (RandomInRange(0.5, 0.8), RandomInRange(0.3, 0.6));
                case "fx": return (RandomInRange(0, 1), RandomInRange(0, 1));
                case "atmo": return (RandomInRange(0.1, 0.3), RandomInRange(0.0, 0.1)); // атмосферные - низкий срез, минимальный резонанс
                case "key": return (RandomInRange(0.6, 0.9), RandomInRange(0.0, 0.2)); // пианино - высокий срез, низкий резонанс
                case "gt": return (RandomInRange(0.4, 0.7), RandomInRange(0.2, 0.5)); // гейтированные - средний срез, умеренный резонанс
                case "sy": return (RandomInRange(0.3, 0.8), RandomInRange(0.1, 0.4)); // синт эмуляции - широкий диапазон
                default: return (RandomInRange(0, 1), RandomInRange(0, 1));
            }
        }

        private struct EnvVals { public double att, dec, sus, rel; }
        private EnvVals GetEnvSettings(string type)
        {
            switch (type)
            {
                case "lead": return new EnvVals { att = RandomInRange(0, 0.02), dec = RandomInRange(0.3, 0.7), sus = RandomInRange(0.6, 1), rel = RandomInRange(0.2, 0.5) };
                case "pad": return new EnvVals { att = RandomInRange(0.1, 0.4), dec = RandomInRange(0.4, 0.8), sus = RandomInRange(0.7, 1), rel = RandomInRange(0.4, 0.8) };
                case "bass": return new EnvVals { att = 0, dec = RandomInRange(0.2, 0.5), sus = RandomInRange(0.3, 0.7), rel = RandomInRange(0.1, 0.3) };
                case "pluck": return new EnvVals { att = 0, dec = RandomInRange(0.1, 0.4), sus = 0, rel = RandomInRange(0.1, 0.3) };
                case "fx": return new EnvVals { att = RandomInRange(0, 0.5), dec = RandomInRange(0, 1), sus = RandomInRange(0, 1), rel = RandomInRange(0, 1) };
                case "atmo": return new EnvVals { att = RandomInRange(0.2, 0.8), dec = RandomInRange(0.5, 1.0), sus = RandomInRange(0.8, 1.0), rel = RandomInRange(0.6, 1.2) }; // медленная атака, долгий релиз
                case "key": return new EnvVals { att = RandomInRange(0, 0.01), dec = RandomInRange(0.2, 0.6), sus = RandomInRange(0.4, 0.8), rel = RandomInRange(0.3, 0.7) }; // быстрая атака как у пианино
                case "gt": return new EnvVals { att = 0, dec = RandomInRange(0.05, 0.2), sus = RandomInRange(0.2, 0.5), rel = RandomInRange(0.05, 0.2) }; // короткие огибающие для гейтов
                case "sy": return new EnvVals { att = RandomInRange(0, 0.1), dec = RandomInRange(0.2, 0.8), sus = RandomInRange(0.3, 0.9), rel = RandomInRange(0.2, 0.6) }; // классические синт настройки
                default: return new EnvVals { att = RandomInRange(0, 0.5), dec = RandomInRange(0, 1), sus = RandomInRange(0, 1), rel = RandomInRange(0, 1) };
            }
        }

        private (List<double> notes, List<double> velocities, List<double> holds) GenerateArpeggio(string pattern, int steps, ArpConfig arpConfig = null)
        {
            // Проверяем, что steps больше 0
            if (steps <= 0)
            {
                steps = 16; // значение по умолчанию
            }
            
            var notes = new List<double>(steps);
            var velocities = new List<double>(steps);
            var holds = new List<double>(steps);

            int baseRoot = RandomInt(-6, 6); // случайный сдвиг тоники (± полоктавы)
            int rotation = RandomInt(0, 7);  // ротация шаблона

            // Получаем шкалу - либо выбранную пользователем, либо случайную
            int[] selectedScale;
            try
            {
                if (arpConfig != null && !string.IsNullOrEmpty(arpConfig.ScaleCategory) && !string.IsNullOrEmpty(arpConfig.ScaleName))
                {
                    selectedScale = ScaleManager.GetScaleAsSemitones(arpConfig.ScaleCategory, arpConfig.ScaleName);
                }
                else
                {
                    selectedScale = ScaleManager.GetRandomScaleAsSemitones(_rnd);
                }
            }
            catch
            {
                selectedScale = null;
            }
            
            // Надежная проверка и fallback
            if (selectedScale == null || selectedScale.Length == 0)
            {
                selectedScale = new[] { 0, 2, 4, 5, 7, 9, 11, 12 }; // мажор
            }
            
            // Дополнительная проверка на случай, если selectedScale все еще пустой
            if (selectedScale == null || selectedScale.Length == 0)
            {
                selectedScale = new[] { 0 }; // минимальная шкала с одной нотой
            }

            // Аккордовые прогрессии (интервалы от тоники)
            var chordProgressions = new Dictionary<string, int[][]>
            {
                ["I-V-vi-IV"] = new[] { new[] {0,4,7}, new[] {7,11,2}, new[] {9,0,4}, new[] {5,9,0} },     // поп-прогрессия
                ["ii-V-I"] = new[] { new[] {2,5,9}, new[] {7,11,2}, new[] {0,4,7}, new[] {0,4,7} },        // джаз
                ["vi-IV-I-V"] = new[] { new[] {9,0,4}, new[] {5,9,0}, new[] {0,4,7}, new[] {7,11,2} },     // альтернатива
                ["I-VII-♭VI-♭VII"] = new[] { new[] {0,4,7}, new[] {10,2,5}, new[] {8,0,3}, new[] {10,2,5} }, // рок
                ["i-♭VII-♭VI-♭VII"] = new[] { new[] {0,3,7}, new[] {10,1,5}, new[] {8,11,3}, new[] {10,1,5} } // минорный рок
            };

            if (string.Equals(pattern, "melody", StringComparison.OrdinalIgnoreCase))
            {
                // Улучшенная мелодическая генерация для Spire
                int octaveSpan = RandomInt(1, 2); // ограничиваем диапазон для Spire
                var melodicIntervals = new int[] { 0, 2, 4, 5, 7, 9, 11 }; // консонантные интервалы
                int lastNote = 0;
                
                for (int i = 0; i < steps; i++)
                {
                    int scaleIdx;
                    if (i == 0)
                    {
                        // Первая нота - тоника или квинта
                        scaleIdx = selectedScale.Length > 0 ? RandomInt(0, Math.Min(2, selectedScale.Length - 1)) : 0;
                    }
                    else
                    {
                        // Последующие ноты - плавные переходы
                        int currentScaleIdx = selectedScale.Length > 0 ? (i - 1 + rotation) % selectedScale.Length : 0;
                        int interval = melodicIntervals.Length > 0 ? melodicIntervals[RandomInt(0, melodicIntervals.Length - 1)] : 0;
                        scaleIdx = selectedScale.Length > 0 ? (currentScaleIdx + interval) % selectedScale.Length : 0;
                        
                        // Избегаем слишком больших скачков
                        if (selectedScale.Length > 0 && Math.Abs(selectedScale[scaleIdx] - lastNote) > 7)
                        {
                            scaleIdx = selectedScale.Length > 0 ? (currentScaleIdx + RandomInt(1, 3)) % selectedScale.Length : 0;
                        }
                    }
                    
                    int scaleNote = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                    int octaveShift = selectedScale.Length > 0 ? (i / selectedScale.Length) % octaveSpan : 0;
                    int semitones = baseRoot + scaleNote + octaveShift * 12;
                    notes.Add(0.5 + semitones * 0.008);
                    lastNote = scaleNote;
                    
                    // Улучшенные динамические акценты для Spire
                    bool isDownbeat = (i % 4) == 0;
                    bool isUpbeat = (i % 8) == 4;
                    bool isCulmination = (i == steps / 2) || (i == steps * 3 / 4);
                    bool isPhraseEnd = (i == steps - 1) || (i == steps - 2);
                    
                    if (isCulmination) velocities.Add(RandomInRange(0.8, 0.95));
                    else if (isDownbeat) velocities.Add(RandomInRange(0.7, 0.85));
                    else if (isUpbeat) velocities.Add(RandomInRange(0.6, 0.75));
                    else if (isPhraseEnd) velocities.Add(RandomInRange(0.5, 0.7)); // затухание
                    else velocities.Add(RandomInRange(0.6, 0.8));
                    
                    // Добавляем holds для более выразительной игры
                    holds.Add((i % 8 == 7 || isPhraseEnd) ? RandomInRange(0.3, 0.7) : 0);
                }
            }
            else if (string.Equals(pattern, "chord", StringComparison.OrdinalIgnoreCase))
            {
                // Аккордовая прогрессия
                var progressionNames = new[] { "I-V-vi-IV", "ii-V-I", "vi-IV-I-V" };
                var selectedProgression = progressionNames.Length > 0 ? chordProgressions[progressionNames[RandomInt(0, progressionNames.Length - 1)]] : chordProgressions["I-V-vi-IV"];
                
                for (int i = 0; i < steps; i++)
                {
                    int chordIdx = (i / 4) % selectedProgression.Length;
                    int noteInChord = i % 3; // играем ноты аккорда по очереди
                    
                    var chord = selectedProgression[chordIdx];
                    int semitones = baseRoot + chord[noteInChord];
                    notes.Add(0.5 + semitones * 0.008);
                    
                    bool isChordChange = (i % 4) == 0;
                    velocities.Add(isChordChange ? RandomInRange(0.8, 0.95) : RandomInRange(0.6, 0.8));
                    holds.Add((i % 8 == 7) ? 1 : 0);
                }
            }
            else if (string.Equals(pattern, "rhythmic", StringComparison.OrdinalIgnoreCase))
            {
                // Улучшенные ритмические паттерны для Spire
                var rhythmPattern = new int[steps];
                var rhythmTypes = new int[][] 
                {
                    new int[] { 1, 0, 1, 0, 1, 0, 1, 0 }, // 4/4 бит
                    new int[] { 1, 0, 0, 1, 0, 0, 1, 0 }, // 3/4 бит  
                    new int[] { 1, 0, 1, 0, 0, 1, 0, 0 }, // синкопа
                    new int[] { 1, 1, 0, 1, 0, 1, 0, 0 }  // сложный ритм
                };
                var selectedRhythm = rhythmTypes.Length > 0 ? rhythmTypes[RandomInt(0, rhythmTypes.Length - 1)] : new int[] { 1, 0, 1, 0, 1, 0, 1, 0 };
                
                // Генерируем мелодические мотивы
                var motifs = new int[][]
                {
                    new int[] { 0, 2, 4, 2 }, // восходящий-нисходящий
                    new int[] { 0, 4, 2, 0 }, // скачок-спуск
                    new int[] { 0, 1, 2, 0 }, // поступенное движение
                    new int[] { 0, 7, 4, 0 }  // квинта-терция
                };
                var selectedMotif = motifs.Length > 0 ? motifs[RandomInt(0, motifs.Length - 1)] : new int[] { 0, 2, 4, 2 };
                
                for (int i = 0; i < steps; i++)
                {
                    int scaleIdx;
                    if (selectedRhythm[i % selectedRhythm.Length] == 1)
                    {
                        // Активная нота - используем мотив
                        int motifIdx = (i / selectedRhythm.Length) % selectedMotif.Length;
                        scaleIdx = selectedScale.Length > 0 && selectedMotif.Length > 0 ? selectedMotif[motifIdx % selectedMotif.Length] % selectedScale.Length : 0;
                    }
                    else
                    {
                        // Пауза или повтор предыдущей ноты
                        if (i > 0 && _rnd.NextDouble() > 0.3)
                        {
                            // Повторяем предыдущую ноту из rhythmPattern
                            scaleIdx = selectedScale.Length > 0 ? RandomInt(0, Math.Min(3, selectedScale.Length - 1)) : 0;
                        }
                        else
                        {
                            scaleIdx = selectedScale.Length > 0 ? RandomInt(0, Math.Min(3, selectedScale.Length - 1)) : 0;
                        }
                    }
                    
                    rhythmPattern[i] = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                }
                
                for (int i = 0; i < steps; i++)
                {
                    int noteOffset = baseRoot + rhythmPattern[i];
                    notes.Add(0.5 + noteOffset * 0.008);
                    
                    // Улучшенные ритмические акценты для Spire
                    bool isDownbeat = (i % 4) == 0;
                    bool isOffbeat = (i % 4) == 2;
                    bool isSyncopation = (i % 8) == 3 || (i % 8) == 6;
                    bool isRhythmActive = selectedRhythm[i % selectedRhythm.Length] == 1;
                    
                    if (!isRhythmActive)
                    {
                        velocities.Add(0); // пауза
                        holds.Add(0);
                    }
                    else if (isDownbeat) 
                    {
                        velocities.Add(RandomInRange(0.8, 0.95));
                        holds.Add(RandomInRange(0.4, 0.8));
                    }
                    else if (isSyncopation) 
                    {
                        velocities.Add(RandomInRange(0.7, 0.85));
                        holds.Add(RandomInRange(0.3, 0.6));
                    }
                    else if (isOffbeat) 
                    {
                        velocities.Add(RandomInRange(0.6, 0.8));
                        holds.Add(RandomInRange(0.2, 0.5));
                    }
                    else 
                    {
                        velocities.Add(RandomInRange(0.5, 0.75));
                        holds.Add(RandomInRange(0.1, 0.4));
                    }
                }
            }
            else if (string.Equals(pattern, "bass", StringComparison.OrdinalIgnoreCase))
            {
                // Улучшенные басовые паттерны для Spire
                var bassPattern = new int[steps];
                var bassTypes = new int[][] 
                {
                    new int[] { 0, 4, 0, 4 }, // root-fifth
                    new int[] { 0, 2, 4, 2 }, // walking bass
                    new int[] { 0, 7, 4, 7 }, // octave jumps
                    new int[] { 0, 1, 2, 0 }  // chromatic approach
                };
                var selectedBassType = bassTypes.Length > 0 ? bassTypes[RandomInt(0, bassTypes.Length - 1)] : new int[] { 0, 4, 0, 4 };
                
                for (int i = 0; i < steps; i++)
                {
                    int scaleIdx = selectedBassType.Length > 0 ? selectedBassType[i % selectedBassType.Length] : 0;
                    if (selectedScale.Length > 0 && scaleIdx >= selectedScale.Length) scaleIdx = selectedScale.Length - 1;
                    bassPattern[i] = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                }
                
                for (int i = 0; i < steps; i++)
                {
                    int semitones = baseRoot + bassPattern[i] - 12; // октавой ниже
                    notes.Add(0.5 + semitones * 0.008);
                    
                    // Улучшенная динамика для баса в Spire
                    bool isDownbeat = (i % 4) == 0;
                    bool isOffbeat = (i % 4) == 2;
                    bool isPhraseEnd = (i == steps - 1);
                    
                    if (isDownbeat) 
                    {
                        velocities.Add(RandomInRange(0.85, 0.95));
                        holds.Add(RandomInRange(0.6, 0.9));
                    }
                    else if (isOffbeat) 
                    {
                        velocities.Add(RandomInRange(0.7, 0.85));
                        holds.Add(RandomInRange(0.4, 0.7));
                    }
                    else if (isPhraseEnd)
                    {
                        velocities.Add(RandomInRange(0.6, 0.8));
                        holds.Add(RandomInRange(0.8, 1.0)); // длинная нота в конце фразы
                    }
                    else 
                    {
                        velocities.Add(RandomInRange(0.6, 0.8));
                        holds.Add(RandomInRange(0.3, 0.6));
                    }
                }
            }
            else
            {
                // Случайная генерация с музыкальной логикой
                for (int i = 0; i < steps; i++)
                {
                    int scaleIdx = selectedScale.Length > 0 ? RandomInt(0, selectedScale.Length - 1) : 0;
                    int semitones = baseRoot + (selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0) + RandomInt(-12, 12);
                    notes.Add(0.5 + semitones * 0.008);
                    velocities.Add(RandomInRange(0.6, 0.95));
                    holds.Add(_rnd.NextDouble() > 0.85 ? 1 : 0);
                }
            }

            return (notes, velocities, holds);
        }

        private (List<double> notes, List<double> velocities, List<double> holds) GenerateStepPattern(int steps, ArpConfig arpConfig = null)
        {
            // Проверяем, что steps больше 0
            if (steps <= 0)
            {
                steps = 16; // значение по умолчанию
            }
            
            var notes = new List<double>(steps);
            var velocities = new List<double>(steps);
            var holds = new List<double>(steps);

            int baseRoot = RandomInt(-6, 6); // случайный сдвиг тоники (± полоктавы)

            // Получаем шкалу - либо выбранную пользователем, либо случайную
            int[] selectedScale;
            try
            {
                if (arpConfig != null && !string.IsNullOrEmpty(arpConfig.ScaleCategory) && !string.IsNullOrEmpty(arpConfig.ScaleName))
                {
                    selectedScale = ScaleManager.GetScaleAsSemitones(arpConfig.ScaleCategory, arpConfig.ScaleName);
                }
                else
                {
                    selectedScale = ScaleManager.GetRandomScaleAsSemitones(_rnd);
                }
            }
            catch
            {
                selectedScale = null;
            }
            
            // Надежная проверка и fallback
            if (selectedScale == null || selectedScale.Length == 0)
            {
                selectedScale = new[] { 0, 2, 4, 5, 7, 9, 11, 12 }; // мажор
            }
            
            // Дополнительная проверка на случай, если selectedScale все еще пустой
            if (selectedScale == null || selectedScale.Length == 0)
            {
                selectedScale = new[] { 0 }; // минимальная шкала с одной нотой
            }
            
            var stepPattern = new int[steps];
            int patternDensity = RandomInt(2, 8); // больше вариаций плотности
            int patternType = RandomInt(0, 2); // 0-sparse начало, 1-scattered, 2-burst
            
            // Генерируем паттерн с учетом плотности
            if (patternType == 0) // разреженный паттерн, больше нот в начале
            {
                for (int i = 0; i < patternDensity && i < steps; i++)
                {
                    if (i == 0 || _rnd.NextDouble() > RandomInRange(0.2, 0.5)) // случайная вероятность
                    {
                        int scaleIdx = selectedScale.Length > 0 ? RandomInt(0, Math.Max(0, selectedScale.Length / 2 - 1)) : 0;
                        stepPattern[i] = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                    }
                }
            }
            else if (patternType == 1) // разбросанные ноты по всему паттерну
            {
                var activePositions = new HashSet<int>();
                activePositions.Add(0); // первая нота всегда активна
                while (activePositions.Count < patternDensity && activePositions.Count < steps)
                {
                    if (steps > 1)
                        activePositions.Add(RandomInt(1, steps - 1));
                }
                foreach (var pos in activePositions)
                {
                    int scaleIdx = selectedScale.Length > 0 ? RandomInt(0, selectedScale.Length - 1) : 0;
                    stepPattern[pos] = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                }
            }
            else // burst - группа нот подряд
            {
                int startPos = steps > patternDensity ? RandomInt(0, steps - patternDensity) : 0;
                for (int i = startPos; i < startPos + patternDensity && i < steps; i++)
                {
                    int scaleIdx = selectedScale.Length > 0 ? RandomInt(0, selectedScale.Length - 1) : 0;
                    stepPattern[i] = selectedScale.Length > 0 ? selectedScale[scaleIdx] : 0;
                }
            }

            // Генерируем случайный паттерн акцентов вместо фиксированного
            var accentPattern = new bool[steps];
            int accentType = RandomInt(0, 4);
            if (accentType == 0) // классический 4/4 бит
            {
                for (int i = 0; i < steps; i++) accentPattern[i] = (i % 4 == 0) || (i % 8 == 4);
            }
            else if (accentType == 1) // нечетные акценты
            {
                for (int i = 0; i < steps; i++) accentPattern[i] = (i % 2 == 1);
            }
            else if (accentType == 2) // случайные акценты с определенной вероятностью
            {
                double accentProbability = RandomInRange(0.25, 0.5);
                accentPattern[0] = true; // первая всегда акцент
                for (int i = 1; i < steps; i++) accentPattern[i] = _rnd.NextDouble() < accentProbability;
            }
            else // фиксированные позиции, но со случайными вариациями
            {
                var fixedPositions = new List<int>();
                int numAccents = RandomInt(2, 6);
                for (int j = 0; j < numAccents && j < steps; j++)
                {
                    int pos = steps > 0 ? RandomInt(0, steps - 1) : 0;
                    fixedPositions.Add(pos);
                }
                foreach (var pos in fixedPositions) accentPattern[pos] = true;
            }

            for (int i = 0; i < steps; i++)
            {
                int noteOffset = baseRoot + stepPattern[i];
                notes.Add(0.5 + noteOffset * 0.008);

                // Используем случайный паттерн акцентов
                bool isAccent = accentPattern[i];
                bool isActive = stepPattern[i] != 0;

                if (isAccent && isActive) velocities.Add(RandomInRange(0.56, 0.85)); // больший диапазон
                else if (isActive) velocities.Add(RandomInRange(0.15, 0.45)); // больший диапазон
                else velocities.Add(0);

                holds.Add(_rnd.NextDouble() > 0.9 ? 1 : 0); // случайные hold'ы
            }

            // Убеждаемся, что все списки заполнены до нужного размера
            while (notes.Count < steps)
            {
                notes.Add(0.5);
                velocities.Add(0.5);
                holds.Add(0);
            }

            return (notes, velocities, holds);
        }

        private double Random01() { return _rnd.NextDouble(); }
        private double RandomInRange(double min, double max) { return min + _rnd.NextDouble() * (max - min); }
        private int RandomInt(int min, int max) { return _rnd.Next(min, max + 1); }
        private double Clamp01(double v) { if (v < 0) return 0; if (v > 1) return 1; return v; }

        private void TryApplySequenceFromTemplate(Dictionary<string, double> target)
        {
            // Удалено: больше не читаем из папки SEQ; секвенции генерируются алгоритмически
        }
    }

    public sealed class PresetFile
    {
        public string Name { get; set; }
        public Preset Preset { get; set; }
    }
} 