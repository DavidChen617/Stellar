    const app = Vue.createApp({
        data() {
            return {
                isRecording: false,
                isProcessing: false,
                mediaRecorder: null,
                audioChunks: [],
                audioBlobUrl: null,
                transcript: '',
                status: '',
                userInput: '',
                selectedImage: null,
                description: '',
                errorMessage: '',
                isUploading: false,
                previewImage: null, // 新增預覽圖片
                fileInputKey: 0, // 用於重置檔案輸入
            };
        },
        computed: {
            buttonClass() {
                return {
                    'record-vue': !this.isRecording && !this.isProcessing,
                    'stop-recording-vue': this.isRecording,
                };
            }
        },
        methods: {
            // 圖片選擇處理
            onImageChange(event) {
                const file = event.target.files[0];
                if (file && file.type.startsWith('image/')) {
                    this.selectedImage = file;
                    this.previewImage = URL.createObjectURL(file); // 創建預覽圖片 URL
                } else {
                    this.selectedImage = null;
                    this.previewImage = null;
                }
            },

            // 上傳圖片並取得文字描述
            async uploadImage() {
                if (!this.selectedImage) {
                    //alert("請選擇一張圖片！");
                    return;
                }

                const formData = new FormData();
                formData.append('imageFile', this.selectedImage);

                try {
                    this.isProcessing = true;
                    this.isUploading = true;
                    this.status = '圖片正在轉文字...';

                    const response = await axios.post('/api/ImageToText/convert', formData, {
                        headers: { 'Content-Type': 'multipart/form-data' }
                    });
                   
                    const { imageUrl, description } = response.data;
                 
                    this.status = '圖片轉文字完成';
                    this.removeImage();
                    this.displayMessage(imageUrl, false, 'image');
                    //this.displayMessage(description, true, 'text');
                    this.callApi("請依照圖片,推薦我遊戲: "+description);
                 
                } catch (error) {
                    console.error("上傳失敗:", error);
                    this.errorMessage = '圖片轉文字失敗，請稍後再試。';
                } finally {
                    this.isUploading = false;
                    this.isProcessing = false;
                }
            },
            removeImage() {
                this.selectedImage = null;
                this.previewImage = null;
                // 清除文件輸入的值
                this.fileInputKey += 1; // 更改 key 以重置檔案輸入

            },
            async toggleRecording() {
                if (this.isRecording) {
                    this.stopRecording();
                } else {
                    await this.startRecording();
                }
            },
            async startRecording() {
                try {

                    // 使用瀏覽器的 getUserMedia API 獲取音頻流
                    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

                    // 創建 MediaRecorder 來處理音頻流，進行錄音
                    this.mediaRecorder = new MediaRecorder(stream);

             
                    // 初始化一個空的陣列來存儲錄製的音頻數據塊
                    this.audioChunks = [];

                    //當錄音結束時，ondataavailable 事件會捕捉錄製的音頻數據並存儲起來。
                    this.mediaRecorder.ondataavailable = (event) => {
                        this.audioChunks.push(event.data);
                    };

                    this.mediaRecorder.onstop = this.handleStop; //暫停時 發送音頻

                    this.mediaRecorder.start();
                    this.isRecording = true;
                    this.status = '錄音中...';
                    this.transcript = '';
                    this.audioBlobUrl = null;
                } catch (error) {
                    console.error('無法訪問麥克風:', error);
                    this.status = '無法訪問麥克風，請檢查權限設置。';
                }
            },
            stopRecording() {
                if (this.mediaRecorder) {
                    this.mediaRecorder.stop();
                    this.isRecording = false;
                    this.status = '錄音已停止，正在上傳...';
                    this.isProcessing = true;
                }
            },
            async handleStop() {

                //將錄音數據 audioChunks 組合成一個 Blob（音頻文件）。

                const audioBlob = new Blob(this.audioChunks, { type: 'audio/wav' });

                // 使用 URL.createObjectURL(audioBlob) 創建一個可以在網頁中播放音頻的 URL
                this.audioBlobUrl = URL.createObjectURL(audioBlob); //   

                this.displayMessage(null, false, "audio", this.audioBlobUrl);
            
                try {
                 
                    const formData = new FormData();

                
                    formData.append('audio', audioBlob, 'recorded_audio.wav');

                    const response = await axios.post('/api/SpeechToText/upload-audio', formData, {
                        headers: {
                            'Content-Type': 'multipart/form-data'
                        }
                    });

                 
                    this.transcript = response.data.transcript;     
                    this.status = '轉換完成。';
                    //將文本 發給ChatGPT (Stellar AI 客服)
                    this.callApi(this.transcript);

             

                } catch (error) {
                    console.error('上傳失敗:', error);
                    this.status = '上傳失敗，請稍後再試。';
                } finally {
                    this.isProcessing = false;
                }
            },
            callApi(userMessage) {
                const apiUrl = '/api/LayoutChat';
            
                //取得回應
                axios.post(apiUrl, { content: userMessage })
                    .then((response) => {
                        //將回應轉換成語音
                        this.convertToSpeech(response.data.response);
                    })

                    .catch((error) => {                                                             
                        console.error('API error:', error);
                    });
            },
            sendMessage() {
                if (this.userInput.trim()) {

                    this.displayMessage(this.userInput, false); 
                    this.userInput = '';
                } else {
                    alert('請輸入訊息');
                }
            },
            displayMessage(message, isFromAssistant, type = 'text', audioBlobUrl = null) {
                const chatContent = document.querySelector('.chat-content');

                const newMessage = document.createElement('p');
                newMessage.classList.add(isFromAssistant ? 'assistant-message' : 'user-message');
             
                if (type === 'image') {
                    // 創建圖片元素
                    const img = document.createElement('img');
                    img.src = message; // 假設 message 是圖片的 URL
                    img.alt = '用戶上傳的圖片';
                    img.classList.add('message-image'); // 添加專用的類名以應用 CSS
                    newMessage.appendChild(img);
                } else if (type === 'audio') {
                    if (isFromAssistant && audioBlobUrl) {
                        let isPlaying = false;
                        const audio = new Audio(audioBlobUrl);
                        newMessage.innerHTML = `${message}`;

                        const micIcon = document.createElement('i');
                        micIcon.classList.add('fa', 'fa-volume-up');
                        micIcon.style.cursor = 'pointer';
                        micIcon.style.position = 'absolute';
                        micIcon.style.right = '-25px';
                        micIcon.style.fontSize = '18px';
                        micIcon.style.color = 'white';

                        micIcon.addEventListener('click', () => {
                            if (isPlaying) {
                                audio.pause();
                                micIcon.style.color = 'white';
                            } else {
                                audio.play();
                                micIcon.style.color = 'red';
                            }
                            isPlaying = !isPlaying;

                        });

                        audio.addEventListener('ended', () => {
                            micIcon.style.color = 'white';
                            isPlaying = false;
                        });
                        newMessage.style.position = 'relative';
                        newMessage.appendChild(micIcon);
                    } else if (audioBlobUrl) {

                        newMessage.innerHTML = `<audio controls src="${audioBlobUrl}" style ="margin-top: 4px; height: 40px;"></audio>`;

                    } 
                }
                else {
                    newMessage.innerHTML = `${message}`;
                }

                chatContent.appendChild(newMessage);
                chatContent.scrollTop = chatContent.scrollHeight;
            
            },
      
            async convertToSpeech(text) {
                try {
                    if (text) {
                        const response = await axios.post('https://api.openai.com/v1/audio/speech', {
                            model: 'tts-1',
                            input: text, 
                            voice: 'alloy'
                            //alloy fable echo nova shimmer onyx
                        }, {
                            headers: {
                                'Authorization': `Bearer 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`,
                                'Content-Type': 'application/json'
                            },
                            responseType: 'blob'
                        });

                        const audioBlob = new Blob([response.data], { type: 'audio/wav' });
                        const audioUrl = URL.createObjectURL(audioBlob);

                 
                        this.displayMessage(text, true, "audio", audioUrl);

                    } else {
                        console.log('無法轉換：沒有可用的 AI 訊息');
                    }
                } catch (error) {
                    console.error('轉換失敗:', error);
                }
            }


        }
    });

    app.mount('#chatBoxForVue');