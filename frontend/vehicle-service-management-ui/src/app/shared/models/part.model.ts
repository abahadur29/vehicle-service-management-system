export interface Part {
  id: number;
  name: string;
  price?: number;
  unitPrice?: number;
  stockQuantity: number;
  isActive?: boolean;
}