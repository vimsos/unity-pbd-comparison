import matplotlib.pyplot as plt
import numpy as np
from runs import get_sorted_relevant_runs
from constants import LEGEND_FONT_SIZE, LATIN_MODERN

RELEVANT_KEYS = {
    ("mono", "Conventional"),
    ("il2cpp", "Conventional"),
    ("il2cpp", "Jobs"),
}

relevant_runs = get_sorted_relevant_runs(RELEVANT_KEYS)

# figure out r bins
range_min = 9999999999999
range_max = -9999999999999
for run in relevant_runs:
    df = run.df_without_outliers
    range_min = min(np.min(df.total), range_min)
    range_max = max(np.max(df.total), range_max)
bins = int((range_max - range_min) * 8)

for size in set([run.get_size() for run in relevant_runs]):
    date_time = relevant_runs[0].get_datetime()
    number_of_runs = relevant_runs[0].get_number_of_runs()
    output_file_name = f"{size}_histogram_{date_time}_{str(number_of_runs)}"
    same_size_runs = [run for run in relevant_runs if run.get_size() == size]

    figure, axis = plt.subplots(ncols=2)

    for arch_index, arch in enumerate(
        sorted(set([run.get_arch() for run in relevant_runs]))
    ):
        same_size_same_arch_runs = [
            run for run in same_size_runs if run.get_arch() == arch
        ]

        # figure out range
        range_min = 9999999999999
        range_max = -9999999999999
        for run in same_size_same_arch_runs:
            df = run.df_without_outliers
            range_min = min(np.min(df.total), range_min)
            range_max = max(np.max(df.total), range_max)
        hist_range = (range_min * 0.975, range_max * 1.025)

        ax = axis[arch_index]
        for run in same_size_same_arch_runs:
            df = run.df_without_outliers
            ax.hist(
                df.total,
                bins=bins,
                range=hist_range,
                density=True,
                alpha=0.75,
                histtype="stepfilled",
                color=run.get_color(),
                label=run.get_name(),
            )

        ax.set_title(f"{run.get_resolution()} {run.get_processor()} {run.get_arch()}")

    figure.legend(
        [
            "mono-Conventional",
            "il2cpp-Conventional",
            "il2cpp-Jobs",
        ],
        loc="upper center",
        bbox_to_anchor=(0.5, -0.05),
        ncol=5,
        fontsize=LEGEND_FONT_SIZE,
    )

    figure.set_figwidth(12.0)
    figure.set_figheight(2.4)
    figure.supxlabel("Tempo (ms)", **LATIN_MODERN)
    figure.supylabel("Número de Ocorrências", **LATIN_MODERN)
    plt.tight_layout(rect=[0, 0, 0.95, 1])
    figure.savefig(output_file_name + ".png", bbox_inches="tight")
