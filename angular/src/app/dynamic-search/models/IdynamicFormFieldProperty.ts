export interface IDynamicFormFieldProperty {
  label?: string;
  controlName: string;
  controlType: string;
  valueType?: string;
  currentValue?: string;
  placeholder?: string;
  multiple?: boolean;
  options?: Array<{
    value: string;
    text: string;
  }>;
  optionsUrl?: string;
  validators?: {
    required?: boolean;
    minlength?: number;
    maxlength?: number;
  };
}
