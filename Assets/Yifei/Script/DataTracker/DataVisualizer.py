import requests
import json
import matplotlib.pyplot as plt
import numpy as np


url = 'https://cureallcrew-default-rtdb.firebaseio.com/CureAllCrew.json'
response = requests.get(url)
data = response.json()


levels = [1, 2, 3, 4, 5]
counts = {level: 0 for level in levels}

for session_id, session_data in data.items():
    if "a" in session_data and "DifficultyLevelReached" in session_data["a"]:
        play_time_str = session_data["a"].get("PlayTime", "0")
        try:
            play_time = float(play_time_str)
        except ValueError:
            continue
        if play_time < 20:
            continue

        level_str = session_data["a"].get("DifficultyLevelReached", "0")
        try:
            level = int(level_str)
        except ValueError:
            continue  
        if level in counts:
            counts[level] += 1


levels_labels = [str(lv) for lv in levels]
counts_data = [counts[lv] for lv in levels]


fig, ax = plt.subplots()
index = np.arange(len(levels_labels))
bar_width = 0.5
ax.bar(index, counts_data, bar_width, color='green') 
ax.set_title('Difficulty Levels Reached')
ax.set_xlabel('Difficulty Level')
ax.set_ylabel('Count')
ax.set_xticks(index)
ax.set_xticklabels(levels_labels)
# plt.savefig("difficulty_levels.png")
plt.show()
