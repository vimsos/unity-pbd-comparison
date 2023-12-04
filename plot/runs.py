import numpy as np
import pandas as pd
import scipy.stats as st
import os
from typing import List
from constants import COLOR_MAP


class Run:
    def __init__(self, filepath: str):
        self.filepath = filepath
        self.filename = filepath.split(os.sep)[-1]
        self.splits = self.filename.split("_")
        self.df = pd.read_csv(filepath)
        # remove outliers
        for c in self.df.columns:
            mean = self.df[c].mean()
            std = self.df[c].std()
            std_distance = 3
            self.df_without_outliers = self.df[
                (abs(self.df[c] - mean) <= std_distance * std)
            ]

    def get_compiler(self):
        return self.splits[0]

    def get_implementation(self):
        return self.splits[1]

    def get_key(self):
        return (self.splits[0], self.splits[1])

    def get_arch(self):
        return self.splits[4].replace("X64", "x86-64")

    def get_processor(self):
        return self.splits[3].replace(
            "11thGenIntelRCoreTMi7-11800H230GHz", "Intel i7-11800H"
        )

    def get_name(self):
        backend = self.splits[0]
        implementation = self.splits[1]
        return f"{backend}-{implementation}"

    def get_color(self):
        return COLOR_MAP.get(self.get_key(), "purple")

    def get_filename(self):
        return self.filename

    def get_datetime(self):
        return self.splits[7].split('.')[0]

    def get_number_of_runs(self):
        return int(self.splits[5])

    def get_size(self):
        return int(self.splits[6])

    def get_resolution(self):
        size = self.get_size()
        return f"{size}x{size}"

    def get_total_time_ms(self):
        return np.sum(self.df.total)

    def get_total_std_deviation(self):
        return np.std(self.df.total)

    def get_total_std_error(self):
        return st.sem(self.df.total)

    def get_total_mean(self):
        return np.mean(self.df.total)

    def get_total_median(self):
        return np.median(self.df.total)

    def get_total_confidence(self, t_star):
        mean = self.get_total_mean()
        std_error = self.get_total_std_error()
        return (mean - t_star * std_error, mean + t_star * std_error)

    def get_gravity_time_ms(self):
        return np.sum(self.df.gravity)

    def get_gravity_std_deviation(self):
        return np.std(self.df.gravity)

    def get_gravity_std_error(self):
        return st.sem(self.df.gravity)

    def get_gravity_mean(self):
        return np.mean(self.df.gravity)

    def get_gravity_median(self):
        return np.median(self.df.gravity)

    def get_gravity_confidence(self, t_star):
        mean = self.get_gravity_mean()
        std_error = self.get_gravity_std_error()
        return (mean - t_star * std_error, mean + t_star * std_error)

    def get_reset_time_ms(self):
        return np.sum(self.df.reset)

    def get_reset_std_deviation(self):
        return np.std(self.df.reset)

    def get_reset_std_error(self):
        return st.sem(self.df.reset)

    def get_reset_mean(self):
        return np.mean(self.df.reset)

    def get_reset_median(self):
        return np.median(self.df.reset)

    def get_reset_confidence(self, t_star):
        mean = self.get_reset_mean()
        std_error = self.get_reset_std_error()
        return (mean - t_star * std_error, mean + t_star * std_error)

    def get_paint_time_ms(self):
        return np.sum(self.df.paint)

    def get_paint_std_deviation(self):
        return np.std(self.df.paint)

    def get_paint_std_error(self):
        return st.sem(self.df.paint)

    def get_paint_mean(self):
        return np.mean(self.df.paint)

    def get_paint_median(self):
        return np.median(self.df.paint)

    def get_paint_confidence(self, t_star):
        mean = self.get_paint_mean()
        std_error = self.get_paint_std_error()
        return (mean - t_star * std_error, mean + t_star * std_error)


def get_all_runs() -> List[Run]:
    all_runs = []
    for subdir, _, files in os.walk("./results"):
        for file in files:
            if file.endswith(".csv"):
                filepath = subdir + os.sep + file
                all_runs.append(Run(filepath))

    return all_runs


def get_sorted_relevant_runs(RELEVANT_KEYS) -> List[Run]:
    relevant_runs = sorted(
        [run for run in get_all_runs() if run.get_key() in RELEVANT_KEYS],
        key=lambda r: r.get_compiler(),
        reverse=True,
    )
    relevant_runs = sorted(relevant_runs, key=lambda r: r.get_implementation())
    relevant_runs = sorted(relevant_runs, key=lambda r: r.get_arch(), reverse=True)
    relevant_runs = sorted(
        relevant_runs,
        key=lambda r: r.get_size(),
    )

    return relevant_runs
