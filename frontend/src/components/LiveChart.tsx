import { Line } from "react-chartjs-2";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  type ChartOptions,
} from "chart.js";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend
);

export type DataPoint = {
  time: string;
  value: number;
};

export default function LiveChart({
  dataPoints,
  title,
  label,
  chartColor,
}: {
  dataPoints: DataPoint[];
  title: string;
  label: string;
  chartColor: string;
}) {
  const data = {
    labels: dataPoints.map((p) => p.time),
    datasets: [
      {
        label: label,
        data: dataPoints.map((p) => p.value),
        borderColor: chartColor,
        backgroundColor: chartColor,
        tension: 0.4,
        pointRadius: 0,
      },
    ],
  };

  const options: ChartOptions<"line"> = {
    responsive: true,
    animation: false,
    plugins: {
      legend: { display: true },
      title: { display: true, text: title },
    },
    scales: {
      x: {
        ticks: {
          maxRotation: 45,
          minRotation: 45,
        },
      },
      y: { beginAtZero: true },
    },
  };

  return (
    <div className="w-full h-full">
      <Line data={data} options={options} />
    </div>
  );
}
