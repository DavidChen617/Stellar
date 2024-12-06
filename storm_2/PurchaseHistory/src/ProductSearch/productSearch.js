
var { ref, onMounted, computed, onBeforeUnmount, watch } = Vue;
const App = {
  setup() {
    const menuVisible = ref(false);
    const errorMessage = ref('');
    const selectedCategories = ref([]); // 選中的分類
    const searchQuery = ref('');
    const priceLimit = ref(100);
    const tags = ref([]);
    const categorys = ref([]);
    const selectedTags = ref([]);
    const products = ref([]);
    const isDragAccordion = ref(false);
    const sortOrder = ref('依照條件排序');
    const isAccordionOpen = ref({
      price: true,
      tags: true,
      categorys: true,
    });
    const currentPage = ref(1);
    const isLoading = ref(false);
    const totalCount = ref(0); 
    const queryTag = ref('');
  
    const showDiscounted = ref(false);
    const hideFreeItems = ref(false);

    const handleClickOutside = (event) => {

      const menu = document.querySelector('.side-menu');
      const toggleButton = document.querySelector('.burger');
      if (!menu.contains(event.target) && !toggleButton.contains(event.target)) {
        menuVisible.value = false;
      }
    };

    const enter = (el, done) => {
      el.offsetWidth; // 觸發重排

      el.style.transform = 'translateX(0)'; // 開始位置

      done(); // 動畫完成後調用 done
    };

    const leave = (el, done) => {

      el.style.transform = 'translateX(100%)'; // 移出螢幕

      done(); // 動畫完成後調用 done
    };

    onMounted(() => {
      document.addEventListener('click', handleClickOutside);
    });




   
    const toggleAccordion = (section) => {

      if (isDragAccordion.value) {
        section.preventDefault();
        return; 
      }
      isAccordionOpen.value[section] = !isAccordionOpen.value[section];
    };


    const preventAccordionToggle = (event) => {
      event.stopPropagation();
    };

    const startDraggingRange = (event) => {
      isDragAccordion.value = true;
      event.stopPropagation()
    };

    const stopDraggingRange = (event) => {
      isDragAccordion.value = false;
      event.stopPropagation();
    };
    onMounted(() => {
      const rangeInput = document.querySelector('input[type="range"]');
      if (rangeInput) {
        rangeInput.addEventListener('pointerdown', startDraggingRange);
        rangeInput.addEventListener('pointerup', stopDraggingRange);
      }
    });
    onBeforeUnmount(() => {
      const rangeInput = document.querySelector('input[type="range"]');
      if (rangeInput) {
        rangeInput.removeEventListener('pointerdown', startDraggingRange);
        rangeInput.removeEventListener('pointerup', stopDraggingRange);
      }
    });

    //==============================
    const getQueryParam = (param) => {
      const urlParams = new URLSearchParams(window.location.search);

      return urlParams.get(param);
    };


    const fetchProductData = async (query, page) => {
      try {
      
        isLoading.value = true;
        errorMessage.value = '加載中...'; 
        await new Promise(resolve => setTimeout(resolve, 800)); 
        const endpoint = query ?
          `https://localhost:7168/api/ProductSearchAPI/GetProductData/${encodeURIComponent(query)}?page=${page}&pageSize=10` :
          `https://localhost:7168/api/ProductSearchAPI/GetProductData?keyword=&page=${page}&pageSize=10`;
        const response = await axios.get(endpoint);
        queryTag.value = query || '';  
        const { tags: fetchedTags, products: fetchedProducts, categorys: fetchedCategorys } = response.data;
        tags.value = fetchedTags;
        products.value.push(...fetchedProducts);
        totalCount.value=response.data.totalCount;
        categorys.value = fetchedCategorys;
     
      } catch (error) {
        console.error('Error fetching product data:', error);

      } finally {
        isLoading.value = false;
      }
    };

    const query = getQueryParam('query');
    const handleScroll = () => {
      const scrollY = window.scrollY;
      const windowHeight = window.innerHeight;
      const documentHeight = document.documentElement.scrollHeight;

      if (scrollY + windowHeight >= documentHeight - 100 && !isLoading.value && products.value.length < totalCount.value) {
        currentPage.value++;
        fetchProductData(query, currentPage.value);
      }
    };
    onMounted(() => {
      fetchProductData(query, currentPage.value);
      window.addEventListener('scroll', handleScroll);
    });

    const clearQueryTag = () => {
      queryTag.value = '';
      fetchProductData('');
    };
    
    const filteredProducts = computed(() => {
      return products.value.filter(product => {
        const matchesCategory = selectedCategories.value.length === 0 || selectedCategories.value.includes(product.categoryId);
        const matchesTags = selectedTags.value.length === 0 ||
          selectedTags.value.some(selectedTagId => product.tagId.includes(selectedTagId));
        const matchesPrice = product.totalPrice <= actualPrice.value;
        const matchesSearchQuery = product.productName.toLowerCase().includes(searchQuery.value.toLowerCase());

        const hasDiscount = product.hasDiscount;
        const isFree = product.totalPrice <= 0;

        return matchesPrice && matchesSearchQuery && matchesCategory && matchesTags &&
          (!showDiscounted.value || hasDiscount) &&
          (!hideFreeItems.value || !isFree);
        ;
      }).sort((a, b) => {
        if (sortOrder.value === 'startDate') {
          return new Date(a.startdate) - new Date(b.startdate);
        } else if (sortOrder.value === 'lowestPrice') {
          return a.totalPrice - b.totalPrice;
        } else if (sortOrder.value === 'highestPrice') {
          return b.totalPrice - a.totalPrice;
        }
        return 0;
      });;
    });

    //==============================
    const totalProductsCount = computed(() => {
      return products.value.length;
    });

    const excludedProductsCount = computed(() => {
      return totalProductsCount.value - filteredProducts.value.length;
    });

    const productCount = computed(() => {
      return filteredProducts.value.length;
    });


    const handleDiscountedChange = (event) => {

      showDiscounted.value = event.target.checked;
    };

    const handleHideFreeItemsChange = (event) => {

      hideFreeItems.value = event.target.checked;
    };


    const toggleCategory = (categoryId) => {
      if (selectedCategories.value.includes(categoryId)) {
        selectedCategories.value = selectedCategories.value.filter(id => id !== categoryId);
      } else {
        selectedCategories.value.push(categoryId);
      }
    };
    const toggleTag = (tagId) => {
      if (selectedTags.value.includes(tagId)) {
        selectedTags.value = selectedTags.value.filter(id => id !== tagId);
      } else {
        selectedTags.value.push(tagId);
      }
    };

    const handleSortChange = (event) => {
      sortOrder.value = event.target.value;
    };

    const actualPrice = computed(() => {
      return Math.round( (priceLimit.value / 10) * 1800);
    });



    const draggedIndex = ref(null);
    const dragOverIndex = ref(null);

    const isDragging = ref(false);

    const onDragStart = (index) => {
      console.log(index)
      draggedIndex.value = index;
      isDragging.value = true;
      document.querySelectorAll('li')[index].classList.add('dragging');
    };
    const resetDragState = () => {
      draggedIndex.value = null;
      dragOverIndex.value = null;
      isDragging.value = false;

    };

    const onDragOver = (index, event) => {
      event.preventDefault();
      event.stopPropagation();

      const target = event.currentTarget.getBoundingClientRect();
      const offset = event.clientY - target.top;
      const threshold = target.height / 2;

      if (offset > threshold) {
        dragOverIndex.value = index + 1;
      } else {
        dragOverIndex.value = index;
      }
    };

    const onDragEnter = (index, event) => {
      event.preventDefault();
      event.stopPropagation();
      dragOverIndex.value = index;
    };

    const onDragLeave = (event) => {
      event.preventDefault();
      event.stopPropagation();
      dragOverIndex.value = null;
    };

    const onDrop = (index, event) => {
      event.preventDefault();
      event.stopPropagation();

      if (draggedIndex.value !== null && draggedIndex.value >= 0 && draggedIndex.value < filteredProducts.value.length) {
        const draggedItem = filteredProducts.value[draggedIndex.value];

        if (draggedItem) {
          filteredProducts.value.splice(draggedIndex.value, 1);
          filteredProducts.value.splice(index, 0, draggedItem);
          // products = [...filteredProducts.value];
        } else {
          console.error('Dragged item is undefined');
        }
      } else {

        console.error('Invalid dragged index');
      }
      resetDragState();
    };

    return {
      productCount,
      totalProductsCount,
      excludedProductsCount,
      priceLimit,
      selectedTags,
      tags,
      categorys,
      products,
      isAccordionOpen,
      filteredProducts,
      actualPrice,
      searchQuery,
      isDragging,
      draggedIndex,
      dragOverIndex,
      queryTag,
      hideFreeItems,
      showDiscounted,
      menuVisible,
      isLoading,
      currentPage,
      totalCount,
      enter,
      leave,
      toggleTag,
      toggleCategory,
      startDraggingRange,
      stopDraggingRange,
      toggleAccordion,
      preventAccordionToggle,
      onDragStart,
      onDragOver,
      resetDragState,
      onDrop,
      onDragEnter,
      onDragLeave,
      resetDragState,
      handleSortChange,
      handleDiscountedChange,
      handleHideFreeItemsChange,
      clearQueryTag,
    };
  }
};
const app1 = Vue.createApp(App);
app1.mount('#productSearch');





