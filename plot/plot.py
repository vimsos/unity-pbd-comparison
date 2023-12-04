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

for size in set([run.get_size() for run in relevant_runs]):
    date_time = relevant_runs[0].get_datetime()
    number_of_runs = relevant_runs[0].get_number_of_runs()
    output_file_name = f"{size}_cumulative_{date_time}_{str(number_of_runs)}"

    figure, axis = plt.subplots(ncols=2)

    for arch_index, arch in enumerate(
        sorted(set([run.get_arch() for run in relevant_runs]))
    ):
        runs = [
            run
            for run in relevant_runs
            if run.get_size() == size and run.get_arch() == arch
        ]

        ax = axis[arch_index]
        max_x = 0.0
        max_y = 0.0
        for run in runs:
            cumulative = [0]
            df = run.df.total / 1000.0
            for i in range(np.size(df)):
                cumulative.append(cumulative[i] + df[i])
            max_x = max(len(cumulative), max_x)
            max_y = max(cumulative[-1], max_y)
            ax.plot(cumulative, color=run.get_color(), label=run.get_name())

        x_60 = np.linspace(start=0, stop=max_x, num=2)
        y_60 = x_60 / 60.0
        x_30 = np.linspace(start=0, stop=max_x, num=2)
        y_30 = x_30 / 30.0
        ax.plot(
            x_30, y_30, color="brown", label="30 quados por segundo", linestyle="dashed"
        )
        ax.plot(
            x_60,
            y_60,
            color="orange",
            label="60 quados por segundo",
            linestyle="dashed",
        )
        ax.set_xlim([0.0, max_x * 1.025])
        ax.set_ylim([0.0, max_y * 1.025])
        ax.set_title(f"{run.get_resolution()} {run.get_processor()} {run.get_arch()}")

    figure.legend(
        [
            "mono-Conventional",
            "il2cpp-Conventional",
            "il2cpp-Jobs",
            "60 quados por segundo",
            "30 quados por segundo",
        ],
        loc="upper center",
        bbox_to_anchor=(0.5, -0.05),
        ncol=5,
        fontsize=LEGEND_FONT_SIZE,
    )

    figure.set_figwidth(12.0)
    figure.set_figheight(2.4)
    figure.supxlabel("Turnos de simulação", **LATIN_MODERN)
    figure.supylabel("Tempo (s)", **LATIN_MODERN)
    plt.tight_layout(rect=[0, 0, 0.95, 1])
    figure.savefig(output_file_name + ".png", bbox_inches="tight")
