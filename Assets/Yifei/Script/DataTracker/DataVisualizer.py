import requests
import json
import matplotlib.pyplot as plt
import numpy as np
from collections import defaultdict

url = 'https://cureallcrew-default-rtdb.firebaseio.com/CureAllCrew.json'
response = requests.get(url)
data = response.json()

print("Valid Session IDs:")
print("-" * 50)

# Create subplots
fig, ((ax1, ax2), (ax3, ax4)) = plt.subplots(2, 2, figsize=(15, 12))
fig.suptitle('Game Data Analysis', fontsize=16)

# 1. Difficulty Level Analysis
levels = [1, 2, 3, 4, 5]
level_counts = {level: 0 for level in levels}

# 2. Buff Selection Analysis
buff_selections = defaultdict(int)

# 3. Skill Idle Duration Analysis
skill_idle_times = defaultdict(list)

# 4. Teammate Damage Analysis
teammate_damage = defaultdict(list)

for session_id, session_data in data.items():
    if "a" not in session_data:
        continue
        
    # Print valid session ID
    print(f"Session ID: {session_id}")
    
    # Difficulty Level Count
    if "DifficultyLevelReached" in session_data["a"]:
        level_str = session_data["a"].get("DifficultyLevelReached", "0")
        try:
            level = int(level_str)
            if level in level_counts:
                level_counts[level] += 1
        except ValueError:
            continue

    # Buff Selection Count
    if "BuffSelections" in session_data["a"]:
        for buff, count in session_data["a"]["BuffSelections"].items():
            buff_selections[buff] += count

    # Skill Idle Duration Count
    if "SkillIdleDuration" in session_data["a"]:
        for skill, ratio in session_data["a"]["SkillIdleDuration"].items():
            skill_idle_times[skill].append(ratio)

    # Teammate Damage Count
    if "TeammateDamage" in session_data["a"]:
        for teammate, damage in session_data["a"]["TeammateDamage"].items():
            teammate_damage[teammate].append(damage)

print("-" * 50)
print(f"Total valid sessions: {len([k for k, v in data.items() if 'a' in v])}")

# 1. Plot Difficulty Level Chart
levels_labels = [str(lv) for lv in levels]
counts_data = [level_counts[lv] for lv in levels]
ax1.bar(levels_labels, counts_data, color='green')
ax1.set_title('Difficulty Level Distribution')
ax1.set_xlabel('Difficulty Level')
ax1.set_ylabel('Player Count')

# 2. Plot Buff Selection Chart
buff_names = list(buff_selections.keys())
buff_counts = list(buff_selections.values())
ax2.bar(buff_names, buff_counts, color='blue')
ax2.set_title('Buff Selection Statistics')
ax2.set_xlabel('Buff Type')
ax2.set_ylabel('Selection Count')
plt.setp(ax2.get_xticklabels(), rotation=45, ha='right')

# 3. Plot Skill Idle Duration Chart
skill_names = list(skill_idle_times.keys())
skill_means = [np.mean(times) for times in skill_idle_times.values()]
ax3.bar(skill_names, skill_means, color='orange')
ax3.set_title('Average Skill Idle Duration Ratio')
ax3.set_xlabel('Skill Type')
ax3.set_ylabel('Average Idle Duration Ratio')
plt.setp(ax3.get_xticklabels(), rotation=45, ha='right')

# 4. Plot Teammate Damage Chart
teammate_names = list(teammate_damage.keys())
damage_means = [np.mean(damages) for damages in teammate_damage.values()]
ax4.bar(teammate_names, damage_means, color='red')
ax4.set_title('Average Teammate Damage')
ax4.set_xlabel('Teammate Type')
ax4.set_ylabel('Average Damage')
plt.setp(ax4.get_xticklabels(), rotation=45, ha='right')

# Adjust layout
plt.tight_layout()
plt.subplots_adjust(top=0.9)

# Show the charts
plt.show()
