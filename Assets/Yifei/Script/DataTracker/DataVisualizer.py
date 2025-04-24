import requests
import json
import matplotlib.pyplot as plt
import seaborn as sns
import numpy as np
from collections import defaultdict

# Set Seaborn style
sns.set(style="whitegrid")

# Data source
url = 'https://cureallcrew-default-rtdb.firebaseio.com/CureAllCrew.json'
response = requests.get(url)
data = response.json()

# Filter thresholds
cutoff_date = 20250408  # Only include sessions after April 8, 2025
min_play_time = 20      # Only include sessions with PlayTime >= 20 seconds

# Teammate name mapping
teammate_name_map = {
    "AxeTeammate_prefab": "MeleeTeammate_3",
    "SwordTeammate_prefab": "MeleeTeammate_2"
}

# Initialize statistics containers
levels = [1, 2, 3, 4, 5]
level_counts = {level: 0 for level in levels}
buff_selections = defaultdict(int)
skill_idle_times = defaultdict(list)
teammate_damage = defaultdict(list)

def extract_date(session_id):
    try:
        return int(session_id.split('-')[0])
    except:
        return 0

valid_sessions = 0

# Process and filter data
for session_id, session_data in data.items():
    if "a" not in session_data:
        continue

    session_date = extract_date(session_id)
    play_time = float(session_data["a"].get("PlayTime", 0))

    if session_date <= cutoff_date or play_time < min_play_time:
        continue

    valid_sessions += 1

    # Difficulty level
    try:
        level = int(session_data["a"].get("DifficultyLevelReached", "0"))
        if level in level_counts:
            level_counts[level] += 1
    except ValueError:
        continue

    # Buff selections
    for buff, count in session_data["a"].get("BuffSelections", {}).items():
        buff_selections[buff] += count

    # Skill idle durations (exclude amplifyheal)
    for skill, ratio in session_data["a"].get("SkillIdleDuration", {}).items():
        if "amplifyheal" in skill.lower():
            continue
        skill_idle_times[skill].append(ratio)

    # Teammate damage with name mapping
    for teammate, damage in session_data["a"].get("TeammateDamage", {}).items():
        mapped_teammate = teammate_name_map.get(teammate, teammate)
        teammate_damage[mapped_teammate].append(damage)

print(f"Total valid sessions (after April 8, PlayTime > 20s): {valid_sessions}")

# Plotting
fig, axs = plt.subplots(2, 2, figsize=(16, 12))
fig.suptitle('Game Session Analysis (Post-April 8)', fontsize=18)

# 1. Difficulty Level Distribution
sns.barplot(x=list(map(str, levels)), y=[level_counts[lvl] for lvl in levels], palette="Greens_d", ax=axs[0, 0])
axs[0, 0].set_title("Difficulty Level Distribution")
axs[0, 0].set_xlabel("Difficulty Level")
axs[0, 0].set_ylabel("Player Count")

# 2. Buff Selection Frequency
buffs = list(buff_selections.keys())
counts = list(buff_selections.values())
sns.barplot(x=buffs, y=counts, palette="Blues_d", ax=axs[0, 1])
axs[0, 1].set_title("Buff Selection Frequency")
axs[0, 1].set_xlabel("Buff Type")
axs[0, 1].set_ylabel("Selection Count")
axs[0, 1].tick_params(axis='x', rotation=30)

# 3. Skill Idle Time Distribution (Box Plot)
skill_data = []
for skill, values in skill_idle_times.items():
    for v in values:
        skill_data.append((skill, v))
skill_df = {"Skill": [x[0] for x in skill_data], "IdleRatio": [x[1] for x in skill_data]}
sns.boxplot(x=skill_df["Skill"], y=skill_df["IdleRatio"], palette="Oranges", ax=axs[1, 0])
axs[1, 0].set_title("Skill Idle Time Ratio Distribution")
axs[1, 0].set_xlabel("Skill Type")
axs[1, 0].set_ylabel("Idle Time Ratio")
axs[1, 0].tick_params(axis='x', rotation=30)

# 4. Teammate Damage Distribution (Violin Plot)
teammate_data = []
for t, values in teammate_damage.items():
    for v in values:
        teammate_data.append((t, v))
team_df = {"Teammate": [x[0] for x in teammate_data], "Damage": [x[1] for x in teammate_data]}
sns.violinplot(x=team_df["Teammate"], y=team_df["Damage"], palette="Reds", ax=axs[1, 1])
axs[1, 1].set_title("Teammate Damage Distribution")
axs[1, 1].set_xlabel("Teammate Type")
axs[1, 1].set_ylabel("Damage Value")
axs[1, 1].tick_params(axis='x', rotation=30)

# Layout adjustment
plt.tight_layout(rect=[0, 0, 1, 0.96])
plt.show()
