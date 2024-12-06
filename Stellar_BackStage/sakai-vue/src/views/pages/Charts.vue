<script setup>
import { getAllChartsService } from "@/service/ChartsService";
import { ref , onMounted} from "vue";
import Chart from 'primevue/chart';




// 變數設置
const chartCGPData = ref(null);
const chartWGCCData = ref(null);
const chartPNData = ref(null);
const chartCGOData = ref(null);


const chartCGPOptions = ref({
  responsive: true,
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
    },
  },
});

const chartWGCCOptions = ref({
  responsive: true,
  scales: {
    r: {
      angleLines: {
        display: false,       // 顯示角度線（雷達圖中的射線）
        color: '#888',       // 設定角度線的顏色
        lineWidth: 1         // 設定角度線的寬度
      },
      grid: {
        display: true,       // 顯示格線
        color: '#ccc',       // 設定格線顏色
        lineWidth: 1         // 設定格線寬度
      },
      ticks: {
        display: true,       // 顯示座標軸標籤
        backdropColor: 'transparent' // 設定標籤背景為透明，避免被遮蓋
      }
    }
  },
  plugins: {
    legend: {
      display: true,
      position: 'bottom',
    }
  }
});

const canvas = document.createElement('canvas');
const ctx = canvas.getContext('2d');

// 定義漸層色彩 (linear gradient)
const gradientBig = ctx.createLinearGradient(0, 0, 0, 400);
gradientBig.addColorStop(0, 'rgba(255, 99, 132, 0.5)');
gradientBig.addColorStop(1, 'rgba(54, 162, 235, 0.5)');
const gradientSmall = ctx.createLinearGradient(0, 0, 0, 400);
gradientSmall.addColorStop(0, 'rgba(51, 255, 255, 0.5)');
gradientSmall.addColorStop(1, 'rgba(51, 255, 51, 0.5)');



// const chartPNOptions = ref({
//   responsive: true,
//   plugins: {
//     legend: {
//       display: true,
//       position: 'bottom',
//     },
//   },
// });
const chartPNOptions = ref({
  responsive: true,
  plugins: {
    legend: {
      display: true,
      position: 'bottom', // 設定圖例在圖表下方
      labels: {
        // 自定義圖例生成方式
        generateLabels: function (chart) {
          const data = chart.data;
          if (data.datasets.length) {
            // 遍歷圖表中的每個標籤
            return data.labels.map((paymentType, index) => {
              const dataset = data.datasets[0];
              const backgroundColor = dataset.backgroundColor[index];

              // 根據 paymentType 值設定圖例標籤文字
              let displayLabel = '';
              if (index === 0) {
                displayLabel = '綠界'; // 當 paymentType 為 1 時顯示「綠界」
              } else if (index === 1) {
                displayLabel = 'Line Pay'; // 當 paymentType 為 2 時顯示「Line Pay」
              } else {
                displayLabel = ''; // 不顯示其他的情況
              }

              // 調試輸出
              console.log(`Index: ${index}, paymentType: ${paymentType}, displayLabel: ${displayLabel}`);

              // 只有當 displayLabel 有值時才回傳圖例
              if (displayLabel) {
                return {
                  text: displayLabel, // 圖例的文字
                  fillStyle: backgroundColor, // 使用與資料區域相同的顏色
                  strokeStyle: dataset.borderColor || 'transparent', // 圖例邊框顏色
                  lineWidth: 0, // 圖例邊框寬度
                  hidden: isNaN(dataset.data[index]), // 若資料為空則隱藏圖例
                  index: index
                };
              } else {
                return null; // 沒有顯示的情況下返回 null
              }
            }).filter(Boolean); // 移除 null 的圖例
          }
          return [];
        }
      },
    }
  }
});



//   responsive: true,
//   plugins: {
//     legend: {
//       display: true,
//       position: 'bottom', // 設定圖例在圖表下方
//       labels: {
//         // 自定義圖例生成方式
//         generateLabels: function (chart) {
//           const data = chart.data;
//           if (data.datasets.length) {
//             // 遍歷圖表中的每個標籤
//             return data.labels.map((paymentType, index) => {
//               const dataset = data.datasets[0];
//               const backgroundColor = dataset.backgroundColor[index];
              
//               // 根據 paymentType 值設定圖例標籤文字
//               let displayLabel = '';
//               switch (paymentType) {
//                 case '1': // 假設 paymentType 是字串
//                   displayLabel = '綠界'; // 當 paymentType 為 1 時顯示「綠界」
//                   break;
//                 case '2': // 假設 paymentType 是字串
//                   displayLabel = 'Line Pay'; // 當 paymentType 為 2 時顯示「Line Pay」
//                   break;
//                 default:
//                   displayLabel = `其他付款方式 (${paymentType})`; // 其他值顯示「其他付款方式」
//               }

//               return {
//                 text: displayLabel, // 圖例的文字
//                 fillStyle: backgroundColor, // 使用與資料區域相同的顏色
//                 strokeStyle: dataset.borderColor || 'transparent', // 圖例邊框顏色
//                 lineWidth: 0, // 圖例邊框寬度
//                 hidden: isNaN(dataset.data[index]), // 若資料為空則隱藏圖例
//                 index: index
//               };
//             });
//           }
//           return [];
//         }
//       }
//     }
//   }
// });




// 定義 Chart.js 選項
// const chartPNOptions = ref({
//   responsive: true,
//   plugins: {
//     legend: {
//       display: false // 禁用預設圖例
//     },
//   },
//   // 自定義插件，使用 afterDraw 方法來繪製自定義圖例
//   plugins: [{
//     id: 'custom-legend',
//     afterDraw: (chart) => {
//       const { ctx, data, chartArea } = chart;
//       const { datasets } = data;

//       if (!datasets || datasets.length === 0) return;

//       const legendX = chartArea.left + 10; // 圖例開始的 X 座標
//       const legendY = chartArea.bottom + 300; // 圖例開始的 Y 座標
//       const legendWidth = 20; // 每個圖例項目的寬度
//       const legendHeight = 20; // 每個圖例項目的高度
//       const padding = 10; // 每個項目之間的間距

//       // 繪製每個圖例項目
//       data.labels.forEach((label, index) => {
//         const backgroundColor = datasets[0].backgroundColor[index];

//         // 繪製漸層圖例方塊
//         const gradient = ctx.createLinearGradient(0, 0, 0, 400);
//         gradient.addColorStop(0, 'rgba(255, 99, 132, 0.5)');
//         gradient.addColorStop(1, 'rgba(54, 162, 235, 0.5)'); // 漸層顏色需根據需求改變
        
//         ctx.fillStyle = gradient; // 設定為漸層
//         ctx.fillRect(legendX, legendY + index * (legendHeight + padding), legendWidth, legendHeight);

//         // 繪製文字標籤
//         ctx.fillStyle = 'white';
//         ctx.font = '14px Arial';
//         ctx.fillText(
//           label,
//           legendX + legendWidth + 10, // 圖例方塊右邊 10px 的位置顯示標籤
//           legendY + index * (legendHeight + padding) + legendHeight - 4 // Y 座標調整以顯示在圖例旁邊
//         );
//       });
//     }
//   }]
// });




// const chartCGOOptions = ref({
//   responsive: true,
//   indexAxis: 'y',
//   maintainAspectRatio: false,
//   aspectRatio: 0.8,
//   plugins: {
//     legend: {
//       display: false,
//       position: 'bottom',
//     },
//   },
// });
const chartCGOOptions = ref({
  responsive: true,
  indexAxis: 'y', // 垂直圖表排列
  maintainAspectRatio: true,
  aspectRatio: 0.8,
  plugins: {
    legend: {
      display: false,          // 開啟圖例顯示
      position: 'bottom',     // 圖例顯示在下方
      labels: {
        font: {
          size: 14           // 調整圖例字體大小
        }
      }
    },
    tooltip: {
      enabled: true           // 啟用工具提示（hover 顯示提示資訊）
    }
  },
  scales: {
    x: {
      display: true,           // 顯示 X 軸
      title: {
        display: true,
        text: '擁有人數'        // X 軸標題
      },
      ticks: {
        font: {
          size: 12            // X 軸標籤字體大小
        }
      },
      grid: {
        display: true          // 顯示格線
      }
    },
    y: {
      display: true,           // 顯示 Y 軸
      title: {
        display: true,
        text: '遊戲類別'       // Y 軸標題
      },
      ticks: {
        font: {
          size: 12            // Y 軸標籤字體大小
        }
      },
      grid: {
        display: true          // 顯示格線
      }
    }
  }
});


onMounted(() => {
  // 使用服務取得後端的圖表資料
  getAllChartsService.getCharts().then((data) => {
    // 從後端取得的資料中提取 'chart_CGP' 這個陣列
    
    
    // 構建 chartCGP 的結構
    const chartCGP = data.chart_CGP;
    // 假設 chart_CGP 是包含 categoryName 和 eachCatePurchaseTimes 的物件陣列
    const chartCGPlabels = chartCGP.map(item => item.categoryName); // 提取 categoryName 作為標籤
    const chartCGPdatasetData = chartCGP.map(item => item.eachCatePurchaseTimes); // 提取 eachCatePurchaseTimes 作為數據
    chartCGPData.value = {
      labels: chartCGPlabels, // 圓餅圖的標籤 
      datasets: [
        {
          data: chartCGPdatasetData, // 每個類別對應的購買次數        
          backgroundColor: [
            getComputedStyle(document.documentElement).getPropertyValue('--p-indigo-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-purple-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-teal-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-orange-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-cyan-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-green-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-blue-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-pink-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-yellow-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-red-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-gray-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-brown-500')
          ],
          hoverBackgroundColor: [
            getComputedStyle(document.documentElement).getPropertyValue('--p-indigo-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-purple-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-teal-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-orange-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-cyan-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-green-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-blue-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-pink-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-yellow-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-red-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-gray-400'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-brown-400')
          ]
        }
      ]
    };

    
    // 處理 chartWGCC 圖表的資料
    const chartWGCC = data.chart_WGCC;
    const chartWGCClabels = chartWGCC.map(item => item.categoryName); // 提取類別標籤
    const chartWGCCdatasetData = chartWGCC.map(item => item.allWishlistEachCateGameNum); // 提取願望清單遊戲數量
    chartWGCCData.value = {
      labels: chartWGCClabels, // 類別標籤
      datasets: [
        {
          data: chartWGCCdatasetData,
          backgroundColor: [
          'rgba(75, 192, 192, 0.5)', // 青色
          'rgba(192, 75, 192, 0.5)', // 紫色
          'rgba(192, 192, 75, 0.5)', // 黃色
          'rgba(75, 75, 192, 0.5)',  // 藍色
          'rgba(192, 75, 75, 0.5)',  // 紅色
          'rgba(255, 159, 64, 0.5)', // 橙色
          'rgba(54, 162, 235, 0.5)', // 淡藍色
          'rgba(153, 102, 255, 0.5)',// 淡紫色
          'rgba(255, 205, 86, 0.5)', // 淡黃
          'rgba(201, 203, 207, 0.5)' // 淡灰色
            // getComputedStyle(document.documentElement).getPropertyValue('--p-indigo-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-purple-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-teal-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-orange-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-cyan-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-green-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-blue-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-pink-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-yellow-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-red-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-gray-500'),
            // getComputedStyle(document.documentElement).getPropertyValue('--p-brown-500')
          ],
          borderColor: [
          'rgba(75, 192, 192, 0)',  // 邊框顏色設為不透明
          'rgba(192, 75, 192, 0)',
          'rgba(192, 192, 75, 0)',
          'rgba(75, 75, 192, 0)',
          'rgba(192, 75, 75, 0)',
          'rgba(255, 159, 64, 0)',
          'rgba(54, 162, 235, 0)',
          'rgba(153, 102, 255, 0)',
          'rgba(255, 205, 86, 0)',
          'rgba(201, 203, 207, 0)'
        ],
          //label: 'Wishlist Count' // 資料標籤
        }
      ]
    };

    // 處理 chartPN 圖表的資料
    const chartPN = data.chart_PN;
    const chartPNlabels  = chartPN.map(item => {
                              if (item.paymentType === 1) {
                              return '綠界';
                              } else if (item.paymentType === 2) {
                              return 'LinePay';
                            } else {
                              return '未知支付方式'; 
                            }
                            }); 
    const chartPNdatasetData = chartPN.map(item => item.eachPaymentNum); 


    
    chartPNData.value = {
      labels: chartPNlabels, // 類別標籤
      datasets: [
        {
          
          data: chartPNdatasetData,
          //backgroundColor: [gradient, gradient, gradient], // 使用漸層填充
          backgroundColor: [
            gradientBig, gradientSmall
          ],
          //backgroundColor: gradients.map(gradient => gradient(ctx)), // 使用漸層填充

          borderColor: '#fff', // 設定邊框顏色
          borderWidth: 0,
        }
      ]
    };

    // 處理 chartCGO 圖表的資料
    const chartCGO = data.chart_CGO;
    const chartCGOlabels = chartCGO.map(item => item.categoryName); 
    const chartCGOdatasetData = chartCGO.map(item => item.eachCateOwnerNum); 
    
    chartCGOData.value = {
      labels: chartCGOlabels, // 類別標籤
      datasets: [
        {
          //label: chartCGOlabels,
          data: chartCGOdatasetData,
          backgroundColor: [
            getComputedStyle(document.documentElement).getPropertyValue('--p-indigo-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-purple-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-teal-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-orange-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-cyan-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-green-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-blue-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-pink-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-yellow-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-red-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-gray-500'),
            getComputedStyle(document.documentElement).getPropertyValue('--p-brown-500')
          ],      
        }
      ]
    };



  });
});
</script>


<template>
  <Fluid class="grid grid-cols-12 gap-8">
      <!-- 1.各類別遊戲的擁有人數 Polar Area -->
      <div class="col-span-12 xl:col-span-6">
        <div class="card flex flex-col items-center h-[500px]">
          <div class="font-semibold text-xl mb-4">各類別遊戲之擁有人數占比</div>
            <Chart type="pie" :data="chartCGPData" :options="chartCGPOptions" />
          </div>        
      </div>
      <!-- 2.擁有人數最多的遊戲(前10 )  直線圖Vertical Bar -->
      <div class="col-span-12 xl:col-span-6">
        <div class="card flex flex-col items-center h-[500px]">
          <div class="font-semibold text-xl mb-4">擁有人數最多的遊戲(前10名)</div>
            <Chart type="bar" :data="chartCGOData" :options="chartCGOOptions" />
          </div>        
      </div>

      <!-- 4.願望清單內遊戲/類別 Horizontal Bar -->
      <div class="col-span-12 xl:col-span-6">
        <div class="card flex flex-col items-center h-[500px]">
          <div class="font-semibold text-xl mb-4">願望清單內各類別遊戲數量</div>
            <Chart type="polarArea" :data="chartWGCCData" :options="chartWGCCOptions" class="w-full md:w-[30rem]" />
          </div>        
      </div>

      <!-- 6.各付款方式人數 Horizontal Bar -->      
      <div class="col-span-12 xl:col-span-6">
        <div class="card flex flex-col items-center h-[500px]">
          <div class="font-semibold text-xl mb-4">各付款方式人數占比</div>
            <Chart type="doughnut" :data="chartPNData" :options="chartPNOptions" class="w-full md:w-[30rem]" />
          </div>        
      </div>
  </Fluid>
</template>






  
<!-- <style scoped>
  /* 圖表樣式調整 */
  h3 {
    text-align: center;
    margin-bottom: 20px;
  }
</style> -->
  