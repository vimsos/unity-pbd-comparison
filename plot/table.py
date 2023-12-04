from tabulate import tabulate
from runs import get_sorted_relevant_runs
from constants import T_STAR

RELEVANT_KEYS = {
    ("mono", "Conventional"),
    ("il2cpp", "Conventional"),
    ("il2cpp", "Jobs"),
    ("il2cpp", "ParallelJobs"),
}

num_format = "{:.4f}"
floatfmt = ".4f"
relevant_runs = get_sorted_relevant_runs(RELEVANT_KEYS)
dictionary = {}

for size in set([run.get_size() for run in relevant_runs]):
    table = []
    table.append(
        [
            "tamanho",
            "cpu",
            "compilador",
            "implementação",
            "total (ms)",
            "média (ms)",
            "mediana (ms)",
            "erro padrão (ms)",
            "desvio padrão (ms)",
            "intervalo confiança (ms)",
            "speedup",
        ]
    )

    same_size_runs = [run for run in relevant_runs if run.get_size() == size]

    for r in same_size_runs:
        confidence = r.get_total_confidence(T_STAR)
        lower = num_format.format(confidence[0])
        upper = num_format.format(confidence[1])
        confidence_as_text = f"({lower}, {upper})"
        (compiler, impl) = r.get_key()
        arch = r.get_arch()

        dictionary[
            (r.get_arch(), r.get_size(), r.get_compiler(), r.get_implementation())
        ] = r
        base = dictionary[(arch, size, "mono", "Conventional")]

        table.append(
            [
                r.get_resolution(),
                arch,
                compiler,
                impl,
                r.get_total_time_ms(),
                r.get_total_mean(),
                r.get_total_median(),
                r.get_total_std_error(),
                r.get_total_std_deviation(),
                confidence_as_text,
                base.get_total_time_ms() / r.get_total_time_ms(),
            ]
        )

    print(tabulate(table, headers="firstrow", tablefmt="fancy_grid", floatfmt=floatfmt))

    with open(f"{size}-table-total", "w") as f:
        print(
            tabulate(table, headers="firstrow", tablefmt="latex", floatfmt=floatfmt),
            file=f,
        )
