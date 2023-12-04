from tabulate import tabulate
from runs import get_sorted_relevant_runs
from constants import T_STAR


RELEVANT_KEYS = {
    ("il2cpp", "Jobs"),
    ("il2cpp", "ParallelJobs"),
}

num_format = "{:.4f}"
floatfmt = ".4f"


table = []
table.append(
    [
        "tamanho",
        "cpu",
        "compilador",
        "implementação",
        "gravity total (ms)",
        "média (ms)",
        "mediana (ms)",
        "erro padrão (ms)",
        "desvio padrão (ms)",
        "intervalo confiança (ms)",
        "speedup",
    ]
)


dictionary = {}
for r in get_sorted_relevant_runs(RELEVANT_KEYS):
    confidence = r.get_gravity_confidence(T_STAR)
    lower = num_format.format(confidence[0])
    upper = num_format.format(confidence[1])
    confidence_as_text = f"({lower}, {upper})"
    (compiler, impl) = r.get_key()
    arch = r.get_arch()
    size = r.get_size()

    dictionary[
        (r.get_arch(), r.get_size(), r.get_compiler(), r.get_implementation())
    ] = r
    base = dictionary[(arch, size, "il2cpp", "Jobs")]

    table.append(
        [
            r.get_resolution(),
            arch,
            compiler,
            impl,
            r.get_gravity_time_ms(),
            r.get_gravity_mean(),
            r.get_gravity_median(),
            r.get_gravity_std_error(),
            r.get_gravity_std_deviation(),
            confidence_as_text,
            base.get_gravity_time_ms() / r.get_gravity_time_ms(),
        ]
    )

print(tabulate(table, headers="firstrow", tablefmt="fancy_grid", floatfmt=floatfmt))

with open("table-parallel-gravity", "w") as f:
    print(
        tabulate(table, headers="firstrow", tablefmt="latex", floatfmt=floatfmt),
        file=f,
    )
